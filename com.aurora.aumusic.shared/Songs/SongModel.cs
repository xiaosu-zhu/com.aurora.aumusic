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
using System;
using System.Runtime.Serialization;

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
