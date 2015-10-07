using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Foundation;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;

namespace com.aurora.aumusic
{
    public class AlbumItem
    {
        public string AlbumName;
        public BitmapImage AlbumArtWork;
        private uint _year = 0;
        public uint Year
        {
            get
            {
                return _year;
            }
            set
            {
                if (value > _year)
                    _year = value;
            }
        }
        public string[] AlbumArtists = null;
        public string[] Artists = null;
        public string[] Genres = null;
        public uint Rating = 0;
        public List<Song> Songs = new List<Song>();


        public void getArtwork()
        {
            if (Songs.Count != 0)
            {
                if (Songs[0].Album != "Unknown Album")
                {
                    string a = Songs[0].ArtWork;
                    AlbumArtWork = new BitmapImage(new Uri(a));
                }
                else AlbumArtWork = new BitmapImage(new Uri("ms-appx:///Assets/unknown.png"));
            }
        }

        public void getArtists()
        {
            if (Songs.Count != 0)
            {
                int i = 0, j = 1, k = 0, m = 0, o = 0;
                foreach (var item in Songs)
                {
                    if (item.AlbumArtists != null)
                    {
                        if (item.AlbumArtists.Length > i)
                        {
                            i = item.AlbumArtists.Length;
                            k = j;
                        }
                    }
                    if (item.Artists != null)
                    {
                        if (item.Artists.Length > m)
                        {
                            m = item.Artists.Length;
                            o = j;
                        }
                    }

                    j++;
                }
                if (k != 0)
                {
                    if (Songs[k - 1].AlbumArtists[0] != null)
                    {
                        List<string> l = new List<string>();
                        foreach (var item in Songs[k - 1].AlbumArtists)
                        {
                            l.Add(item);
                        }
                        AlbumArtists = l.ToArray();
                    }
                }

                if (o != 0)
                {
                    if (Songs[o - 1].Artists[0] != null)
                    {
                        List<string> l = new List<string>();
                        foreach (var item in Songs[o - 1].Artists)
                        {
                            l.Add(item);
                        }
                        Artists = l.ToArray();
                    }
                }

                if (AlbumArtists == null)
                {
                    try
                    {
                        Artists.CopyTo(AlbumArtists, 0);
                    }
                    catch (Exception)
                    {
                        AlbumArtists[0] = Artists[0] = "Unknown Artists";
                    }
                }
                if (Artists == null)
                {
                    try
                    {
                        AlbumArtists.CopyTo(AlbumArtists, 0);
                    }
                    catch (Exception)
                    {
                        AlbumArtists[0] = Artists[0] = "Unknown Artists";
                    }
                }
            }
        }

        public void getYear()
        {
            if (Songs.Count != 0)
            {
                foreach (var item in Songs)
                {
                    Year = item.Year;
                }
            }
        }

        public void Initial()
        {
            if (Songs.Count != 0)
            {
                this.getArtists();
                this.getArtwork();
                this.getYear();
            }
        }
    }
}
