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
using com.aurora.aumusic.shared;
using com.aurora.aumusic.shared.Songs;
using Windows.Media.Playback;
using Windows.UI.Xaml.Controls;

namespace com.aurora.aumusic
{
    public class PlaybackControl
    {
        public Grid PlaybackControlGrid;
        AppBarButton PlayPauseButton;
        AppBarButton ShuffleButton;
        Image AlbumArtwork;
        TextBlock TimeRemainingBlock;
        TextBlock TimePastBlock;
        Slider ProgressSlider;

        public PlaybackControl(Grid grid)
        {
            PlaybackControlGrid = grid;
            PlayPauseButton = PlaybackControlGrid.FindName("PlayPauseButtonOnLeft") as AppBarButton;
            ShuffleButton = PlaybackControlGrid.FindName("ShuffleButton") as AppBarButton;
            AlbumArtwork = PlaybackControlGrid.FindName("PlayBackImage") as Image;
            TimeRemainingBlock = PlaybackControlGrid.FindName("TimeRemainingBlock") as TextBlock;
            TimePastBlock = PlaybackControlGrid.FindName("TimeElapsedBlock") as TextBlock;
            ProgressSlider = PlaybackControlGrid.FindName("ProgressSlider") as Slider;
        }

        public void setPlaybackControl(SongModel currentSong)
        {
            DurationValueConverter converter = new DurationValueConverter();
            TimeRemainingBlock.Text = (string)converter.Convert(currentSong.Duration, null, null, null);
        }

        public void setPlaybackControlDefault()
        {
            PlayPauseButton.Icon = new SymbolIcon(Symbol.Play);
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            var t = (TextBlock)ToolTipService.GetToolTip(PlayPauseButton);
            t.Text = loader.GetString("PlayTip.Text");
            TimePastBlock.Text = "0:00";
            TimeRemainingBlock.Text = "0:00";
            AlbumArtwork.Source = null;
        }

        public void setPlaybackControl(MediaPlayerState state)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            var t = (TextBlock)ToolTipService.GetToolTip(PlayPauseButton);
            switch (state)
            {
                case MediaPlayerState.Closed:
                    break;
                case MediaPlayerState.Opening:
                    break;
                case MediaPlayerState.Buffering:
                    break;
                case MediaPlayerState.Playing:
                    ProgressSlider.IsEnabled = true;
                    (PlayPauseButton.FindName("PlayPauseSymbolLeft") as SymbolIcon).Symbol = Symbol.Pause;
                    t.Text = loader.GetString("PauseTip.Text");
                    break;
                case MediaPlayerState.Paused:
                    (PlayPauseButton.FindName("PlayPauseSymbolLeft") as SymbolIcon).Symbol = Symbol.Play;
                    t.Text = loader.GetString("PlayTip.Text");
                    break;
                case MediaPlayerState.Stopped:
                    ProgressSlider.IsEnabled = false;
                    (PlayPauseButton.FindName("PlayPauseSymbolLeft") as SymbolIcon).Symbol = Symbol.Play;
                    t.Text = loader.GetString("PlayTip.Text");
                    break;
                default:
                    break;
            }
        }
    }
}
