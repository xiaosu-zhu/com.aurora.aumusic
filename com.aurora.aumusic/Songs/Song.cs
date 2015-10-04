
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using System;
using Windows.Storage.AccessCache;
using TagLib;
using System.IO;

namespace com.aurora.aumusic
{
    public class Song
    {
        private string _title;
        private string _album;
        public string[] Artists = null;
        public string[] AlbumArtists = null;
        public BitmapImage Artwork = null;
        public uint Rating = 0;
        public int MainKey = 0;
        public StorageFile AudioFile = null;
        public uint Disc = 0;
        public uint DiscCount = 0;
        public String[] Genres = null;
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


        public Song()
        {

        }
        public Song(StorageFile File)
        {
            this.AudioFile = File;
        }

        public async Task<Tag> SetTags()
        {
            this.MainKey = AudioFile.Name.GetHashCode();
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
                    default:
                        return null;
                }
            }
            return null;
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
            return tags;
        }

        private async Task GetArtWorks(Tag tags)
        {
            IPicture[] p = tags.Pictures;
            if (p.Length > 0)
            {
                StorageFolder cacheFolder = ApplicationData.Current.LocalFolder;
                if (p[0].MimeType.Contains("png"))
                {
                    StorageFile cacheImg = await cacheFolder.CreateFileAsync(tags.Album + ".png", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBytesAsync(cacheImg, p[0].Data.Data);
                    Artwork = new BitmapImage(new Uri("ms-appdata:///local/" + tags.Album + ".png"));
                }
                if (p[0].MimeType.Contains("jpeg"))
                {
                    StorageFile cacheImg = await cacheFolder.CreateFileAsync(tags.Album + ".jpg", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBytesAsync(cacheImg, p[0].Data.Data);
                    Artwork = new BitmapImage(new Uri("ms-appdata:///local/" + tags.Album + ".jpg"));
                }
            }

        }

        private async Task<Tag> SetTagMP3()
        {
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
            return tags;
        }


        public static async Task<List<Song>> GetSongListfromPath(string tempPath)
        {
            List<Song> SongList = new List<Song>();
            List<String> TempTypeStrings = new List<string> { ".mp3", ".m4a", ".flac", ".wav" };
            StorageFolder TempFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(tempPath);
            IReadOnlyList<IStorageFile> tempList = await TempFolder.GetFilesAsync();
            foreach (StorageFile tempFile in tempList)
            {
                if (TempTypeStrings.Contains(tempFile.FileType))
                {
                    Song song = new Song(tempFile);
                    var tags = await song.SetTags();
                    await song.GetArtWorks(tags);
                    SongList.Add(song);
                }
            }

            return SongList;
        }
    }
}