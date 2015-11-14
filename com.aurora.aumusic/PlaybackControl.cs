using com.aurora.aumusic.shared.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Xaml.Controls;

namespace com.aurora.aumusic
{
    public class PlaybackControl
    {
        public Grid PlaybackControlGrid;
        Button PlayPauseButton;
        Button ShuffleButton;
        Image AlbumArtwork;
        TextBlock TimeRemainingBlock;
        TextBlock TimeElapsedBlock;

        public PlaybackControl(Grid grid)
        {
            PlaybackControlGrid = grid;
            PlayPauseButton = PlaybackControlGrid.FindName("PlayPauseButtonOnLeft") as Button;
            ShuffleButton = PlaybackControlGrid.FindName("ShuffleButton") as Button;
            AlbumArtwork = PlaybackControlGrid.FindName("PlayBackImage") as Image;
            TimeRemainingBlock = PlaybackControlGrid.FindName("TimeRemainingBlock") as TextBlock;
            TimeElapsedBlock = PlaybackControlGrid.FindName("TimeElapsedBlock") as TextBlock;
        }

        public void setPlaybackControl(SongModel currentSong)
        {

        }

        public void setPlaybackControlDefault()
        {
            
        }

        public void setPlaybackControl(MediaPlayerState state)
        {
            
        }
    }
}
