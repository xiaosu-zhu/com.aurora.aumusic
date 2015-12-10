
namespace com.aurora.aumusic.shared.Songs
{
    interface ISong
    {

        string[] Artists { get; set; }

        string[] AlbumArtists { get; set; }

        string ArtWork { get; set; }

        uint Rating { get; set; }

        string MainKey { get; set; }

        uint Disc { get; set; }

        uint DiscCount { get; set; }

        string[] Genres { get; set; }

        uint TrackCount { get; set; }

        uint Year { get; set; }

        bool Loved { get; set; }

        string AlbumName { get; set; }

        string Title { get; set; }
    }
}
