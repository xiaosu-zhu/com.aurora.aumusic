using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace com.aurora.aumusic.shared.Songs
{
    [DataContract]
    public class SongModel
    {
        [DataMember]
        public string Title;
        [DataMember]
        public string Album;
        [DataMember]
        public string AlbumArtwork;
        [DataMember]
        public TimeSpan Duration;
        [DataMember]
        public string MainKey;
        [DataMember]
        public string[] Artists;
        [DataMember]
        public string[] Genres;
        [DataMember]
        public uint Year;
        [DataMember]
        public uint Rating;
        [DataMember]
        public int Position;
        [DataMember]
        public int SubPosition;

        public SongModel(Song song)
        {
            Title = song.Title;
            Album = song.Album;
            AlbumArtwork = song.ArtWork;
            Duration = song.Duration;
            MainKey = song.MainKey;
            Artists = song.Artists;
            Genres = song.Genres;
            Rating = song.Rating;
            Year = song.Year;
            Position = song.Position;
            SubPosition = song.SubPosition;
        }
    }
}
