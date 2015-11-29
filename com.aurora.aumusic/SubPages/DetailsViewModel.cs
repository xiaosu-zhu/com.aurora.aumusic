using com.aurora.aumusic.shared;
using com.aurora.aumusic.shared.MessageService;
using com.aurora.aumusic.shared.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.Storage;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace com.aurora.aumusic
{
    internal class DetailsViewModel : INotifyPropertyChanged
    {
        private string mainkey;
        private ulong fileSize;
        private TimeSpan duration;
        private uint bitRate;
        private string fileType;
        private string title;
        private string album;
        private string[] artists;
        private string[] albumArtists;
        private uint year;
        private string[] genres;
        private uint[] tracks;
        private uint rating;

        internal void Update(SongModel song, uint bitRate, ulong size, string musicType)
        {
            FileSize = size;
            MainKey = song.MainKey;
            Duration = song.Duration;
            BitRate = bitRate;
            FileType = musicType;
            Title = song.Title;
            Album = song.Album;
            Artists = song.Artists;
            AlbumArtists = song.AlbumArtists;
            Year = song.Year;
            Genres = song.Genres;
            Rating = song.Rating;
            Track = new uint[] { song.Track, song.TrackCount, song.Disc, song.DiscCount };
        }

        public DetailsViewModel()
        {

        }

        public ulong FileSize
        {
            get
            {
                return fileSize;
            }

            set
            {
                fileSize = value;
                this.OnPropertyChanged();
            }
        }

        public string MainKey
        {
            get
            {
                return mainkey;
            }

            set
            {
                mainkey = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return duration;
            }

            set
            {
                duration = value;
                OnPropertyChanged();
            }
        }

        public uint BitRate
        {
            get
            {
                return bitRate;
            }

            set
            {
                bitRate = value;
                OnPropertyChanged();
            }
        }

        public string FileType
        {
            get
            {
                return fileType;
            }

            set
            {
                fileType = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                title = value;
                OnPropertyChanged();
            }
        }

        public string Album
        {
            get
            {
                return album;
            }

            set
            {
                
                album = value;
                OnPropertyChanged();
            }
        }

        public string[] Artists
        {
            get
            {
                return artists;
            }

            set
            {
                artists = value;
                OnPropertyChanged();
            }
        }

        public string[] AlbumArtists
        {
            get
            {
                return albumArtists;
            }

            set
            {
                albumArtists = value;
                OnPropertyChanged();
            }
        }

        public uint Year
        {
            get
            {
                return year;
            }

            set
            {
                year = value;
                OnPropertyChanged();
            }
        }

        public string[] Genres
        {
            get
            {
                return genres;
            }

            set
            {
                genres = value;
                OnPropertyChanged();
            }
        }

        public uint[] Track
        {
            get
            {
                return tracks;
            }

            set
            {
                tracks = value;
                OnPropertyChanged();
            }
        }



        public uint Rating
        {
            get
            {
                return rating;
            }

            set
            {
                rating = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}