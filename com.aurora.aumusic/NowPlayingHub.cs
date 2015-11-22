using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.aurora.aumusic.shared.Songs;
using Windows.UI.Xaml.Controls;
using com.aurora.aumusic.shared;

namespace com.aurora.aumusic
{
    class NowPlayingHub
    {
        Grid RootGrid;

        public NowPlayingHub(Grid grid)
        {
            RootGrid = grid;
        }

        internal void Set(SongModel currentSong)
        {
            var text = RootGrid.FindName("NowTitle") as TextBlock;
            text.Text = currentSong.Title;
            text = RootGrid.FindName("NowAlbum") as TextBlock;
            text.Text = currentSong.Album;
            text = RootGrid.FindName("NowArtists") as TextBlock;
            var converter = new ArtistsConverter();
            text.Text = (string)converter.Convert(currentSong.Artists, null, false, null);

        }
    }
}
