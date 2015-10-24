
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System.Text;
using System.Linq;
using Windows.Storage;
using System;
using Windows.Storage.AccessCache;
using TagLib;
using System.IO;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.FileProperties;

namespace com.aurora.aumusic
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
                    if (item == "")
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
                _artWork = (value == null) ? "ms-appx:///Assets/unknown.png" : value;
            }
        }


        public uint Rating = 0;
        public string MainKey = null;
        public StorageFile AudioFile = null;
        public uint Disc = 0;
        public uint DiscCount = 0;
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
        public uint Track = 0;
        public uint TrackCount = 0;
        public uint Year = 0;

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

        internal static Song RestoreSongfromStorage(List<IStorageFile> allList, ApplicationDataContainer SubContainer)
        {
            Song tempSong = new Song();
            bool isExist = false;
            try
            {
                string key = (string)SubContainer.Values["MainKey"];
                string foldertoken = (string)SubContainer.Values["FolderToken"];
                tempSong.MainKey = key;
                tempSong.FolderToken = foldertoken;
                foreach (var item in allList)
                {
                    if (tempSong.MainKey == ((StorageFile)item).Path + ((StorageFile)item).Name)
                    {
                        tempSong.AudioFile = (StorageFile)item;
                        isExist = true;
                        break;
                    }
                }
                if (!isExist)
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
            tempSong.PlayTimes = (int)SubContainer.Values["PlayTimes"];
            tempSong.Rating = (uint)SubContainer.Values["Rating"];
            tempSong.Title = (string)SubContainer.Values["Title"];
            tempSong.ArtWork = (string)SubContainer.Values["ArtWork"];
            tempSong.Album = (string)SubContainer.Values["Album"];
            tempSong.Year = (uint)SubContainer.Values["Year"];
            tempSong.Disc = (uint)SubContainer.Values["Disc"];
            tempSong.DiscCount = (uint)SubContainer.Values["DiscCount"];
            tempSong.Track = (uint)SubContainer.Values["Track"];
            tempSong.TrackCount = (uint)SubContainer.Values["TrackCount"];
            tempSong.AlbumArtists = (((string)SubContainer.Values["AlbumArtists"]) != null ? ((string)SubContainer.Values["AlbumArtists"]).Split(new char[3] { '|', ':', '|' }) : null);
            tempSong.Artists = (((string)SubContainer.Values["Artists"]) != null ? ((string)SubContainer.Values["Artists"]).Split(new char[3] { '|', ':', '|' }) : null);
            tempSong.Genres = (((string)SubContainer.Values["Genres"]) != null ? ((string)SubContainer.Values["Genres"]).Split(new char[3] { '|', ':', '|' }) : null);
            string[] sa = (((string)SubContainer.Values["Duration"]) != null ? ((string)SubContainer.Values["Duration"]).Split(':') : null);
            tempSong.Duration = new TimeSpan(Int32.Parse(sa[0]), Int32.Parse(sa[1]), Int32.Parse(sa[2]));
            return tempSong;
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

        public Song(StorageFile File, string tempPath)
        {
            this.AudioFile = File;
            this.FolderToken = tempPath;
        }




        public Song()
        {
        }

        public Song(StorageFile audioFile)
        {
            AudioFile = audioFile;
        }

        public async Task<Tag> SetTags()
        {
            this.MainKey = AudioFile.Path + AudioFile.Name;
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

        public void SaveSongtoStorage(Song item, uint progress)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer MainContainer =
    localSettings.CreateContainer("song" + progress, ApplicationDataCreateDisposition.Always);
                MainContainer.Values["FolderToken"] = item.FolderToken;
                MainContainer.Values["MainKey"] = item.MainKey;
                MainContainer.Values["Title"] = item.Title;
                MainContainer.Values["ArtWork"] = item.ArtWork;
                MainContainer.Values["Album"] = item.Album;
                MainContainer.Values["Year"] = item.Year;
                MainContainer.Values["Disc"] = item.Disc;
                MainContainer.Values["DiscCount"] = item.DiscCount;
                MainContainer.Values["Track"] = item.Track;
                MainContainer.Values["TrackCount"] = item.TrackCount;
                MainContainer.Values["Rating"] = item.Rating;
                string sb = Artists != null ? string.Join("|:|", Artists) : null;
                MainContainer.Values["Artists"] = sb;
                sb = AlbumArtists != null ? string.Join("|:|", AlbumArtists) : null;
                MainContainer.Values["AlbumArtists"] = sb;
                sb = Genres != null ? string.Join("|:|", Genres) : null;
                MainContainer.Values["Genres"] = sb;
                MainContainer.Values["Duration"] = Duration.ToString();
                MainContainer.Values["PlayTimes"] = PlayTimes;
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
                        await cacheFolder.GetFileAsync(ArtWorkName + ".png");
                        ArtWork = "ms-appdata:///local/" + ArtWorkName + ".png";
                    }
                    if (p[0].MimeType.Contains("jpeg") || p[0].MimeType.Contains("jpg"))
                    {
                        await cacheFolder.GetFileAsync(ArtWorkName + ".jpg");
                        ArtWork = "ms-appdata:///local/" + ArtWorkName + ".jpg";
                    }

                }
                catch (Exception)
                {
                    if (p[0].MimeType.Contains("png"))
                    {
                        StorageFile cacheImg = await cacheFolder.CreateFileAsync(ArtWorkName + ".png", CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteBytesAsync(cacheImg, p[0].Data.Data);
                        ArtWork = "ms-appdata:///local/" + ArtWorkName + ".png";
                    }
                    if (p[0].MimeType.Contains("jpeg") || p[0].MimeType.Contains("jpg"))
                    {
                        StorageFile cacheImg = await cacheFolder.CreateFileAsync(ArtWorkName + ".jpg", CreationCollisionOption.ReplaceExisting);
                        await FileIO.WriteBytesAsync(cacheImg, p[0].Data.Data);
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

            if (!(p.Title.Contains("?") || p.Title == null || p.Title == "" || p.Album.Contains("?") || p.Album == ""))
            {
                this.Title = p.Title;
                this.Album = p.Album;
                this.AlbumArtists = new[] { p.AlbumArtist };
                this.Artists = new[] { p.Artist };
            }
            else
            {
                this.Title = tags.Title;
                this.Album = tags.Album;
                this.AlbumArtists = tags.AlbumArtists;
                this.Artists = tags.Performers;
            }
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