using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;

namespace com.aurora.aumusic
{
    public class AlbumItem
    {
        public List<Song> Songs;
        public string AlbumName;
        public BitmapImage AlbumArtWork;
        public List<Song> AlbumSongs = new List<Song>();
        StorageFolder cacheFolder = ApplicationData.Current.LocalFolder;

        public void getArtwork()
        {
            if(AlbumSongs.Count!=0)
            {
                if (AlbumSongs[0].Album != "Unknown Album")
                {
                    string a = AlbumSongs[0].ArtWork;
                    AlbumArtWork = new BitmapImage(new Uri(a));
                }
                else AlbumArtWork = new BitmapImage(new Uri("ms-appx:///Assets/unknown.png"));
            }
        }
    }
}
