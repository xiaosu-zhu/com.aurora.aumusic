using com.aurora.aumusic.shared;
using com.aurora.aumusic.shared.MessageService;
using com.aurora.aumusic.shared.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace com.aurora.aumusic
{
    public class PlaybackControl
    {
        public Grid PlaybackControlGrid;
        AppBarButton PlayPauseButton;
        AppBarButton ShuffleButton;
        Image AlbumArtwork;
        TextBlock TimeRemainingBlock;
        TextBlock TimeElapsedBlock;

        public PlaybackControl(Grid grid)
        {
            PlaybackControlGrid = grid;
            PlayPauseButton = PlaybackControlGrid.FindName("PlayPauseButtonOnLeft") as AppBarButton;
            ShuffleButton = PlaybackControlGrid.FindName("ShuffleButton") as AppBarButton;
            AlbumArtwork = PlaybackControlGrid.FindName("PlayBackImage") as Image;
            TimeRemainingBlock = PlaybackControlGrid.FindName("TimeRemainingBlock") as TextBlock;
            TimeElapsedBlock = PlaybackControlGrid.FindName("TimeElapsedBlock") as TextBlock;
        }

        public void setPlaybackControl(SongModel currentSong)
        {
            AlbumArtwork.Source = new BitmapImage(currentSong.AlbumArtwork);
            DurationValueConverter converter = new DurationValueConverter();
            TimeRemainingBlock.Text = (string)converter.Convert(currentSong.Duration, null, null, null);

        }

        public void setPlaybackControlDefault()
        {
            PlayPauseButton.Icon = new SymbolIcon(Symbol.Play);
            TimeElapsedBlock.Text = "0:00";
            TimeRemainingBlock.Text = "0:00";
            AlbumArtwork.Source = null;
        }

        public void setPlaybackControl(MediaPlayerState state)
        {
            switch (state)
            {
                case MediaPlayerState.Closed:
                    break;
                case MediaPlayerState.Opening:
                    break;
                case MediaPlayerState.Buffering:
                    break;
                case MediaPlayerState.Playing:
                    (PlayPauseButton.FindName("PlayPauseSymbolLeft") as SymbolIcon).Symbol = Symbol.Pause;
                    break;
                case MediaPlayerState.Paused:
                    (PlayPauseButton.FindName("PlayPauseSymbolLeft") as SymbolIcon).Symbol = Symbol.Play;
                    break;
                case MediaPlayerState.Stopped:
                    break;
                default:
                    break;
            }
        }
    }
}
