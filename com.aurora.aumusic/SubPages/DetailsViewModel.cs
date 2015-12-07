//Copyright(C) 2015 Aurora Studio

//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
//to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
//WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.



/// <summary>
/// Usings
/// </summary>
using com.aurora.aumusic.shared.Songs;
using System;
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