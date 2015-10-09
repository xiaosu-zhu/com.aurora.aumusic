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
using Windows.UI.Xaml;

namespace com.aurora.aumusic
{
    public class AlbumItem
    {
        public string AlbumName;
        public string AlbumArtWork;
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

        public AlbumItem(List<Song> tempSongs)
        {
            this.Songs = tempSongs;
            this.AlbumName = tempSongs[0].Album;
        }

        public AlbumItem()
        {
        }

        public void getArtwork()
        {
            if (Songs.Count != 0)
            {
                if (Songs[0].Album != "Unknown Album")
                {
                    AlbumArtWork = Songs[0].ArtWork;
                }
                else AlbumArtWork = "ms-appx:///Assets/unknown.png";
            }
        }

        public void getArtists()
        {
            if (Songs.Count != 0)
            {
                int i = 0, j = 0, k = 0, m = 0, o = 0;
                foreach (var item in Songs)
                {
                    if (item.AlbumArtists != null)
                    {
                        if ((item.AlbumArtists.Length) != 0)
                        {
                            if (item.AlbumArtists.Length > i)
                            {
                                i = item.AlbumArtists.Length;
                                k = j;
                            }
                        }
                    }
                    if (item.Artists != null)
                    {
                        if (item.Artists.Length != 0)
                        {
                            if (item.Artists.Length > m)
                            {
                                m = item.Artists.Length;
                                o = j;
                            }
                        }
                    }


                    j++;
                }
                if (k != 0)
                {
                    if (Songs[k].AlbumArtists[0] != null)
                    {
                        List<string> l = new List<string>();
                        foreach (var item in Songs[k].AlbumArtists)
                        {
                            l.Add(item);
                        }
                        AlbumArtists = l.ToArray();
                    }
                }

                if (o != 0)
                {
                    if (Songs[o].Artists[0] != null)
                    {
                        List<string> l = new List<string>();
                        foreach (var item in Songs[o].Artists)
                        {
                            l.Add(item);
                        }
                        Artists = l.ToArray();
                    }
                }

                if (AlbumArtists == null)
                {
                    AlbumArtists = Artists == null ? (new[] { "Unknown Artists" }) : Artists;
                }
                if (Artists == null)
                {
                    Artists = AlbumArtists;
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
                this.getGenres();
                this.getArtwork();
                this.getYear();
                this.Sort();
            }
        }

        private void Sort()
        {
            if(Songs.Count != 0)
            {
                Songs.Sort((first, second) =>
                {
                    return first.Disc.CompareTo(second.Disc);
                });
                Songs.Sort((first, second) =>
                {
                    return first.Track.CompareTo(second.Track);
                });
            }
        }

        private void getGenres()
        {
            if (Songs.Count != 0)
            {
                int i = 0, j = 0, k = 0;
                foreach (var item in Songs)
                {
                    if (item.Genres != null)
                    {
                        if (item.Genres.Length != 0)
                        {
                            if (item.Genres.Length > i)
                            {
                                i = item.AlbumArtists.Length;
                                k = j;
                            }
                        }
                    }
                    j++;
                }
                if (k != 0)
                {
                    if (Songs[k].Genres[0] != null)
                    {
                        List<string> l = new List<string>();
                        foreach (var item in Songs[k].Genres)
                        {
                            l.Add(item);
                        }
                        Genres = l.ToArray();
                    }
                }
                else
                {
                    Genres = new[] { "Unknown Genres" };
                }
            }

        }
   
    }
}
