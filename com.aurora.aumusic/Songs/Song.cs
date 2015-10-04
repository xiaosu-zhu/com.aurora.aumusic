
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using System;
using Windows.Storage.AccessCache;
using TagLib;
using System.IO;
using Windows.Storage.Streams;

namespace com.aurora.aumusic
{
    public class Song
    {
        public string Title = null;
        public string Album = null;
        public string[] Artists = null;
        public string[] AlbumArtists = null;
        public BitmapImage Artwork = null;
        public int Rating = 0;
        public Tag Tags = null;
        public int MainKey = 0;
        public StorageFile AudioFile = null;
        public int Disc = 0;
        public int DiscCount = 0;
        public String[] Genres = null;
        public int Track = 0;
        public int TrackCount = 0;
        public int Year = 0;

        public Song()
        {

        }
        public Song(StorageFile File)
        {
            this.AudioFile = File;
        }

        public void SetTags()
        {
            AttachTags();
            this.Title = Tags.Title;
            this.Album = Tags.Album;
            this.AlbumArtists = Tags.AlbumArtists;
            this.Artists = Tags.Performers;
            String s = AudioFile.Name;
            this.MainKey = s.GetHashCode();
        }

        private void AttachTags()
        {
            if (null != this.AudioFile)
            {
                switch (AudioFile.FileType)
                {
                    case ".mp3": SetTagMP3(); break;
                    case ".m4a": SetTagM4A(); break;
                    case ".flac": SetTagFLAC(); break;
                    default:
                        break;
                }

            }
        }

        private async void SetTagFLAC()
        {
            var fileStream = await AudioFile.OpenStreamForReadAsync();
            var tagFile = TagLib.File.Create(new StreamFileAbstraction(AudioFile.Name,
                             fileStream, fileStream));
            var tags = tagFile.GetTag(TagTypes.FlacMetadata);
            Tags = tags;
        }

        private async void SetTagM4A()
        {
            var fileStream = await AudioFile.OpenStreamForReadAsync();
            var tagFile = TagLib.File.Create(new StreamFileAbstraction(AudioFile.Name,
                             fileStream, fileStream));
            var tags = tagFile.GetTag(TagTypes.Apple);
            Tags = tags;
        }

        private async Task GetArtWorks()
        {
            IPicture[] p = Tags.Pictures;
            StorageFolder cacheFolder = ApplicationData.Current.LocalFolder;
            StorageFile cacheImg = await cacheFolder.CreateFileAsync(Tags.Album+".png", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(cacheImg, p[0].Data.Data);
        }

        private async void SetTagMP3()
        {
            var fileStream = await AudioFile.OpenStreamForReadAsync();
            var tagFile = TagLib.File.Create(new StreamFileAbstraction(AudioFile.Name,
                             fileStream, fileStream));
            var tags = tagFile.GetTag(TagTypes.Id3v2);
            Tags = tags;
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
                    song.SetTags();
                    SongList.Add(song);
                }
            }

            return SongList;
        }
    }
}