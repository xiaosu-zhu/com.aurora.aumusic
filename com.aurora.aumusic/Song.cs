using ID3Library;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using System;

namespace com.aurora.aumusic
{
    public class Song
    {
        public string Title;
        public string Album;
        public string Artist;
        public string AlbumArtist;
        public BitmapImage Artwork;
        public int Rating;
        public ID3 id3 = new ID3();
        public int MainKey;
        public StorageFile AudioFile;

        public Song()
        {

        }
        public Song(StorageFile File)
        {
            this.AudioFile = File;
        }

        public async void SetTags()
        {
            await GetID3();
            this.Title = id3.Title;
            this.Album = id3.Album;
            this.AlbumArtist = id3.AlbumArtist;
            this.Artist = id3.Artist;
            this.Rating = id3.Rating;
            this.Artwork = await id3.GetAlbumArtAsync();
            String s = Title + Album + Artist + AlbumArtist;
            this.MainKey = s.GetHashCode();
        }

        private async Task GetID3()
        {
            if (null != this.AudioFile)
            {
                await id3.GetMusicPropertiesAsync(AudioFile);
            }
        }

        public static async Task<List<Song>> GetSongListfromPath(string tempPath)
        {
            List<Song> SongList = new List<Song>();
            List<String> TempTypeStrings = new List<string> { ".mp3", ".aac", ".m4a", ".flac" };
            StorageFolder TempFolder = await StorageFolder.GetFolderFromPathAsync(tempPath);
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