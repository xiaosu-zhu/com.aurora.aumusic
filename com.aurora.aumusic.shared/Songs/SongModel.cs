using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
        public Uri AlbumArtwork;
        [DataMember]
        public TimeSpan Duration;
        [DataMember]
        public string MainKey;

        public SongModel(Song song)
        {
            Title = song.Title;
            Album = song.Album;
            AlbumArtwork = new Uri(song.ArtWork);
            Duration = song.Duration;
            MainKey = song.MainKey;
        }
    }
}
