//Copyright(C) 2015 Aurora Studio

//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
//to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
//WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.



/// <summary>
/// Usings
/// </summary>
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Windows.Storage;
using System;
using TagLib;
using System.IO;
using Windows.Foundation;
using Windows.Storage.FileProperties;
using com.aurora.aumusic.shared.Albums;

namespace com.aurora.aumusic.shared.Songs
{
    public class Song
    {
        private static readonly char[] InvalidFileNameChars = new[] { '"', '<', '>', '|', '\0', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\a', '\b', '\t', '\n', '\v', '\f', '\r', '\u000e', '\u000f', '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001e', '\u001f', ':', '*', '?', '\\', '/' };
        private static readonly string[] tempTypeStrings = new[] { ".mp3", ".m4a", ".flac", ".wav" };
        private string _title;
        private string _album;
        private string[] _artists;
        public string[] Artists
        {
            get
            {
                return _artists;
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    this._artists = new[] { "Unknown Artists" };
                    return;
                }
                List<string> l = new List<string>();
                foreach (var item in value)
                {
                    if (item == "" || item == " " || item == "  ")
                    {
                        continue;
                    }
                    l.Add(item);
                }
                if (l.Count > 0)
                {
                    _artists = l.ToArray();
                    return;
                }
                else
                    _artists = new[] { "Unknown Artists" };
            }
        }
        private string[] _albumartists;
        public string[] AlbumArtists
        {
            get
            {
                return _albumartists;
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    this._albumartists = new[] { "Unknown AlbumArtists" };
                    return;
                }
                List<string> l = new List<string>();
                foreach (var item in value)
                {
                    if (item == "" || item == " " || item == "  ")
                    {
                        continue;
                    }
                    l.Add(item);
                }
                if (l.Count > 0)
                {
                    _albumartists = l.ToArray();
                    return;
                }
                else
                    _albumartists = new[] { "Unknown AlbumArtists" };
            }
        }
        private string _artWork;
        private string _artWorkName;
        public string ArtWorkName
        {
            get
            {
                return _artWorkName;
            }
            set
            {
                value += "";
                value = InvalidFileNameChars.Aggregate(value, (current, c) => current.Replace(c + "", "_"));
                _artWorkName = value;
            }
        }
        public string ArtWork
        {
            get
            {
                return _artWork;
            }
            set
            {
                //不要試圖使用 .scale-XXX 在下載包的時候會被 trim 掉
                _artWork = (value == null) ? "ms-appx:///Assets/ArtworkPlaceholder.png" : value;
            }
        }


        public uint Rating = 0;
        public string MainKey = null;
        public StorageFile AudioFile = null;
        private uint disc = 1;
        public uint DiscCount = 1;
        public string[] _genres;
        public string[] Genres
        {
            get
            {
                return _genres;
            }
            set
            {
                if (value == null || value.Length == 0)
                {
                    this._genres = new[] { "Unknown Genres" };
                }
                else
                {
                    this._genres = value;
                }
            }
        }

        private uint track = 1;
        public uint TrackCount = 1;
        public uint Year = 0;
        public bool Loved = false;
        public string Album
        {
            get
            {
                return _album;
            }
            set
            {
                _album = (value == null) ? "Unknown Album" : value;
            }
        }
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = (value == null) ? AudioFile.DisplayName : value;
            }
        }
        public AlbumItem Parent;

        public static void SaveSongtoStorage(ApplicationDataContainer SubContainer, int j, Song song)
        {
            ApplicationDataContainer triContainer = SubContainer.CreateContainer("Song" + j, ApplicationDataCreateDisposition.Always);
            triContainer.Values["FolderToken"] = song.FolderToken;
            triContainer.Values["MainKey"] = song.MainKey;
            triContainer.Values["Title"] = song.Title;
            triContainer.Values["ArtWork"] = song.ArtWork;
            triContainer.Values["Album"] = song.Album;
            triContainer.Values["Year"] = song.Year;
            triContainer.Values["Disc"] = song.Disc;
            triContainer.Values["DiscCount"] = song.DiscCount;
            triContainer.Values["Track"] = song.Track;
            triContainer.Values["TrackCount"] = song.TrackCount;
            triContainer.Values["Rating"] = song.Rating;
            string sb = string.Join("|:|", song.Artists);
            triContainer.Values["Artists"] = sb;
            sb = string.Join("|:|", song.AlbumArtists);
            triContainer.Values["AlbumArtists"] = sb;
            sb = string.Join("|:|", song.Genres);
            triContainer.Values["Genres"] = sb;
            triContainer.Values["Duration"] = song.Duration;
            triContainer.Values["PlayTimes"] = song.PlayTimes;
            triContainer.Values["Width"] = song.ArtWorkSize.Width;
            triContainer.Values["Height"] = song.ArtWorkSize.Height;
            triContainer.Values["Loved"] = song.Loved;
        }

        internal static Song RestoreSongfromStorage(ApplicationDataContainer SubContainer, int j)
        {
            ApplicationDataContainer triContainer =
    SubContainer.CreateContainer("Song" + j, ApplicationDataCreateDisposition.Always);
            Song tempSong = new Song();
            string key = (string)triContainer.Values["MainKey"];
            string foldertoken = (string)triContainer.Values["FolderToken"];
            tempSong.MainKey = key;
            tempSong.FolderToken = foldertoken;
            tempSong.PlayTimes = (int)triContainer.Values["PlayTimes"];
            tempSong.Rating = (uint)triContainer.Values["Rating"];
            tempSong.Title = (string)triContainer.Values["Title"];
            tempSong.ArtWork = (string)triContainer.Values["ArtWork"];
            tempSong.Album = (string)triContainer.Values["Album"];
            tempSong.Year = (uint)triContainer.Values["Year"];
            tempSong.Disc = (uint)triContainer.Values["Disc"];
            tempSong.DiscCount = (uint)triContainer.Values["DiscCount"];
            tempSong.Track = (uint)triContainer.Values["Track"];
            tempSong.TrackCount = (uint)triContainer.Values["TrackCount"];
            tempSong.AlbumArtists = (((string)triContainer.Values["AlbumArtists"]) != null ? ((string)triContainer.Values["AlbumArtists"]).Split(new char[3] { '|', ':', '|' }) : null);
            tempSong.Artists = (((string)triContainer.Values["Artists"]) != null ? ((string)triContainer.Values["Artists"]).Split(new char[3] { '|', ':', '|' }) : null);
            tempSong.Genres = (((string)triContainer.Values["Genres"]) != null ? ((string)triContainer.Values["Genres"]).Split(new char[3] { '|', ':', '|' }) : null);
            tempSong.Duration = (TimeSpan)triContainer.Values["Duration"];
            tempSong.ArtWorkSize = new Size((double)triContainer.Values["Width"], (double)triContainer.Values["Height"]);
            tempSong.Loved = (bool)triContainer.Values["Loved"];
            return tempSong;
        }

        public static List<IStorageFile> MatchingFiles(List<AlbumItem> albums, List<IStorageFile> allList)
        {
            for (int k = albums.Count - 1; k >= 0; k--)
            {
                for (int j = albums[k].Songs.Count - 1; j >= 0; j--)
                {
                    bool isExist = false;
                    if (albums[k].Songs[j].AudioFile != null)
                    {
                        continue;
                    }
                    else
                    {
                        for (int i = allList.Count - 1; i >= 0; i--)
                        {
                            if (albums[k].Songs[j].MainKey == ((StorageFile)allList[i]).Path)
                            {
                                albums[k].Songs[j].AudioFile = (StorageFile)allList[i];
                                allList.RemoveAt(i);
                                isExist = true;
                                break;
                            }
                        }
                        if (isExist == false)
                        {
                            albums[k].Songs.RemoveAt(j);
                            albums[k].SongsCount = albums[k].Songs.Count;
                        }
                    }
                }
                if (albums[k].Songs.Count == 0)
                    albums.RemoveAt(k);
            }
            return allList;
        }

        public string FolderToken { get; set; }
        public TimeSpan Duration { get; private set; }
        private int _playtimes = 0;
        public int PlayTimes
        {
            get
            {
                return _playtimes;
            }
            set
            {
                _playtimes = value < 0 ? 0 : value;
            }
        }

        public int Position { get; internal set; }
        public int SubPosition { get; internal set; }
        public Size ArtWorkSize { get; private set; }

        public uint Track
        {
            get
            {
                return track;
            }

            set
            {
                if (value == 0)
                {
                    track = 1;
                    return;
                }
                track = value;
            }
        }

        public uint Disc
        {
            get
            {
                return disc;
            }

            set
            {
                if (value == 0)
                {
                    disc = 1;
                    return;
                }
                disc = value;
            }
        }

        public Song(StorageFile File, string tempPath)
        {
            this.AudioFile = File;
            this.FolderToken = tempPath;
        }




        public Song()
        {
        }

        public Song(SongModel song)
        {
            this.Position = song.Position;
            this.Album = song.Album;
            this.Artists = song.Artists;
            this.Year = song.Year;
            this.Title = song.Title;
            this.SubPosition = song.SubPosition;
            this.Rating = song.Rating;
            this.MainKey = song.MainKey;
            this.Duration = song.Duration;
            this.Genres = song.Genres;
            this.ArtWork = song.AlbumArtwork;
        }

        public Song(StorageFile audioFile)
        {
            AudioFile = audioFile;
        }

        public async Task<Tag> SetTags()
        {
            this.MainKey = AudioFile.Path;
            MusicProperties p = await AudioFile.Properties.GetMusicPropertiesAsync();
            TimeSpan D = p.Duration;
            if (D.Milliseconds > 500)
            {
                Duration = new TimeSpan(D.Hours, D.Minutes, D.Seconds + 1);
            }
            else
            {
                Duration = new TimeSpan(D.Hours, D.Minutes, D.Seconds);
            }
            return await AttachTags();
        }

        private async Task<Tag> AttachTags()
        {
            if (null != this.AudioFile)
            {
                switch (AudioFile.FileType)
                {
                    case ".mp3": var tags1 = await SetTagMP3(); return tags1;
                    case ".m4a": var tags2 = await SetTagM4A(); return tags2;
                    case ".flac": var tags3 = await SetTagFLAC(); return tags3;
                    case ".wav": SetTagWav(); return null;
                    default:
                        return null;
                }
            }
            return null;
        }

        private void SetTagWav()
        {
            Album = null;
            Title = null;
            ArtWork = null;
            AlbumArtists = null;
            Artists = null;
            Genres = null;
        }

        private async Task<Tag> SetTagFLAC()
        {
            var fileStream = await AudioFile.OpenStreamForReadAsync();
            var tagFile = TagLib.File.Create(new StreamFileAbstraction(AudioFile.Name,
                             fileStream, fileStream));
            var tags = tagFile.GetTag(TagTypes.FlacMetadata);
            this.Title = tags.Title;
            this.Album = tags.Album;
            this.AlbumArtists = tags.AlbumArtists;
            this.Artists = tags.Performers;
            this.Year = tags.Year;
            this.Genres = tags.Genres;
            this.Disc = tags.Disc;
            this.DiscCount = tags.DiscCount;
            this.Track = tags.Track;
            this.TrackCount = tags.TrackCount;
            return tags;
        }


        private async Task<Tag> SetTagM4A()
        {
            var fileStream = await AudioFile.OpenStreamForReadAsync();
            var tagFile = TagLib.File.Create(new StreamFileAbstraction(AudioFile.Name,
                             fileStream, fileStream));
            var tags = tagFile.GetTag(TagTypes.Apple);
            this.Title = tags.Title;
            this.Album = tags.Album;
            this.AlbumArtists = tags.AlbumArtists;
            this.Artists = tags.Performers;
            this.Year = tags.Year;
            this.Genres = tags.Genres;
            this.Disc = tags.Disc;
            this.DiscCount = tags.DiscCount;
            this.Track = tags.Track;
            this.TrackCount = tags.TrackCount;
            return tags;
        }

        private async Task GetArtWorks(Tag tags)
        {
            ArtWorkName = tags.Album;
            IPicture[] p = tags.Pictures;
            StorageFolder cacheFolder = ApplicationData.Current.LocalFolder;

            if (p.Length > 0)
            {
                try
                {
                    if (p[0].MimeType.Contains("png"))
                    {
                        StorageFile s = await cacheFolder.GetFileAsync(ArtWorkName + ".png");
                        ImageProperties ip = await s.Properties.GetImagePropertiesAsync();
                        ArtWorkSize = new Size(ip.Width, ip.Height);
                        ArtWork = "ms-appdata:///local/" + ArtWorkName + ".png";
                    }
                    if (p[0].MimeType.Contains("jpeg") || p[0].MimeType.Contains("jpg"))
                    {
                        StorageFile s = await cacheFolder.GetFileAsync(ArtWorkName + ".jpg");
                        ImageProperties ip = await s.Properties.GetImagePropertiesAsync();
                        ArtWorkSize = new Size(ip.Width, ip.Height);
                        ArtWork = "ms-appdata:///local/" + ArtWorkName + ".jpg";
                    }

                }
                catch (Exception)
                {
                    if (p[0].MimeType.Contains("png"))
                    {
                        StorageFile cacheImg = await cacheFolder.CreateFileAsync(ArtWorkName + ".png", CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteBytesAsync(cacheImg, p[0].Data.Data);
                        ImageProperties ip = await cacheImg.Properties.GetImagePropertiesAsync();
                        ArtWorkSize = new Size(ip.Width, ip.Height);
                        ArtWork = "ms-appdata:///local/" + ArtWorkName + ".png";
                    }
                    if (p[0].MimeType.Contains("jpeg") || p[0].MimeType.Contains("jpg"))
                    {
                        StorageFile cacheImg = await cacheFolder.CreateFileAsync(ArtWorkName + ".jpg", CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteBytesAsync(cacheImg, p[0].Data.Data);
                        ImageProperties ip = await cacheImg.Properties.GetImagePropertiesAsync();
                        ArtWorkSize = new Size(ip.Width, ip.Height);
                        ArtWork = "ms-appdata:///local/" + ArtWorkName + ".jpg";
                    }
                }

            }
            else
            {
                try
                {
                    StorageFile item = await cacheFolder.GetFileAsync(ArtWorkName);
                }
                catch (Exception)
                {
                    ArtWork = null;
                    ArtWorkSize = new Size(400, 400);
                }
            }
        }

        private async Task<Tag> SetTagMP3()
        {
            MusicProperties p = await AudioFile.Properties.GetMusicPropertiesAsync();
            var fileStream = await AudioFile.OpenStreamForReadAsync();
            var tagFile = TagLib.File.Create(new StreamFileAbstraction(AudioFile.Name,
                             fileStream, fileStream));
            var tags = tagFile.GetTag(TagTypes.Id3v2);
            this.Title = tags.Title;
            this.Album = tags.Album;
            this.AlbumArtists = tags.AlbumArtists;
            this.Artists = tags.Performers;
            this.Year = tags.Year;
            this.Genres = tags.Genres;
            this.Disc = tags.Disc;
            this.DiscCount = tags.DiscCount;
            this.Track = tags.Track;
            this.TrackCount = tags.TrackCount;
            return tags;
        }

        public void PlayOnce()
        {
            this.PlayTimes++;
            ShuffleList.Save(this);
        }

        public async Task initial()
        {
            if (this.AudioFile != null)
            {
                var tags = await this.SetTags();
                if (tags != null)
                {
                    try
                    {
                        await this.GetArtWorks(tags);
                    }
                    catch (Exception)
                    {
                        this.ArtWork = (this.ArtWorkName == null ? this.ArtWork = null : this.ArtWork = "ms-appdata:///local/" + this.ArtWorkName);

                    }

                }
            }

        }

    }
}