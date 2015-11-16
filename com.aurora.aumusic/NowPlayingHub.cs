using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.aurora.aumusic.shared.Songs;
using Windows.UI.Xaml.Controls;

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

        }
    }
}
