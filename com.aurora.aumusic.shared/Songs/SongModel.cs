using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared.Songs
{
    public class SongModel
    {
        public string Title;
        public string Album;
        public Uri AlbumArtwork;
        public TimeSpan Duration;
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
