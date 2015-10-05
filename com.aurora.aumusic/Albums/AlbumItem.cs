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
        public BitmapImage AlbumArtwork;
        private double _height = double.NaN;
        StorageFolder cacheFolder = ApplicationData.Current.LocalFolder;
        public double Height
        {
            get
            {
                if (AlbumArtwork != null)
                {
                    _height = AlbumArtwork.PixelHeight;
                }
                return _height;
            }
        }

        public string Text
        {
            get; set;
        }

        public void getArtwork()
        {
            Uri a = new Uri("ms-appdata:///local/" + AlbumName + ".png");
            AlbumArtwork = new BitmapImage(a);
            
        }
    }
}
