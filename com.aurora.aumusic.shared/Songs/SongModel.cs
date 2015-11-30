using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
        [DataMember]
        public uint Track;
        [DataMember]
        public uint TrackCount;
        [DataMember]
        public uint Disc;
        [DataMember]
        public uint DiscCount;
        [DataMember]
        public string[] AlbumArtists;
        [DataMember]
        public string FolderToken;
        [DataMember]
        public int PlayTimes;
        [DataMember]
        public bool Loved;

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
            Track = song.Track;
            TrackCount = song.TrackCount;
            DiscCount = song.DiscCount;
            Disc = song.Disc;
            AlbumArtists = song.AlbumArtists;
            FolderToken = song.FolderToken;
            PlayTimes = song.PlayTimes;
            Loved = song.Loved;
        }
    }
}
