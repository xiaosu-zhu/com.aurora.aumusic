
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
        public string Title;
        public string Album;
        public string[] Artists;
        public string[] AlbumArtists;
        public BitmapImage Artwork;
        public int Rating;
        public Tag Tags;
        public int MainKey;
        public StorageFile AudioFile;
        public int Disc;
        public int DiscCount;
        public String[] Genres;
        public int Track;
        public int TrackCount;
        public int Year;

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
            //this.Title = Tags.Title;
            //this.Album = Tags.Album;
            //this.AlbumArtists = Tags.AlbumArtists;
            //this.Artists = Tags.Performers;
            //String s = Title + Album + Artist + AlbumArtist;
            //this.MainKey = s.GetHashCode();
        }

        private void AttachTags()
        {
            if (null != this.AudioFile)
            {
                //await id3.GetMusicPropertiesAsync(AudioFile);
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

        private void SetTagFLAC()
        {
            throw new NotImplementedException();
        }

        private async void SetTagM4A()
        {
            var fileStream = await AudioFile.OpenStreamForReadAsync();
            var tagFile = TagLib.File.Create(new StreamFileAbstraction(AudioFile.Name,
                             fileStream, fileStream));
            var tags = tagFile.GetTag(TagTypes.Apple);
            Tags = tags;
            await GetArtWorks();
        }

        private async Task GetArtWorks()
        {
            IPicture[] p = Tags.Pictures;
            StorageFolder cacheFolder = ApplicationData.Current.LocalFolder;
            StorageFile cacheImg = await cacheFolder.CreateFileAsync(Tags.Album+".png", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(cacheImg, p[0].Data.Data);
        }

        private void SetTagMP3()
        {
            throw new NotImplementedException();
        }

        public static async Task<List<Song>> GetSongListfromPath(string tempPath)
        {
            List<Song> SongList = new List<Song>();
            List<String> TempTypeStrings = new List<string> { ".mp3", ".aac", ".m4a", ".flac" };
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