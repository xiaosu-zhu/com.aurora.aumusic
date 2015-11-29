using com.aurora.aumusic.shared;
using com.aurora.aumusic.shared.MessageService;
using com.aurora.aumusic.shared.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.Storage.Streams;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.Storage;

namespace com.aurora.aumusic
{

    public sealed partial class NowPage : Page
    {
        SongModel CurrentSong;
        Color MainColor;
        DetailsViewModel SongDetails;

        private bool _loved = false;
        private bool _broken = false;
        private ThreadPoolTimer wtftimer;

        private bool Broken
        {
            get
            {
                return _broken;
            }
        }

        private bool Loved
        {
            get
            {
                return _loved;
            }
            set
            {
                if (_loved == true && value == false)
                    _broken = true;
                if (_loved == false && value == true)
                    _broken = false;
                _loved = value;
            }
        }

        public MediaPlayerState NowState = MediaPlayerState.Playing;
        private PlaybackMode NowMode = PlaybackMode.Normal;
        private double volume;

        public NowPage()
        {
            this.InitializeComponent();
            this.SongDetails = new DetailsViewModel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
                CurrentSong = e.Parameter as SongModel;
            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            updateui();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundMediaPlayer_MessageReceivedFromBackground;
            if (wtftimer != null)
            {
                wtftimer.Cancel();
                wtftimer = null;
            }
            CurrentSong = null;
        }

        private async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {

            BackPlaybackChangedMessage message;
            if (MessageService.TryParseMessage(e.Data, out message))
            {
                if (message.CurrentSong != null)
                {
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, new Windows.UI.Core.DispatchedHandler(() =>
                     {
                         CurrentSong = message.CurrentSong;
                         updateui();
                     }));
                }
                NowState = message.NowState;
            }
            UpdateArtworkMessage artwork;
            if (MessageService.TryParseMessage(e.Data, out artwork))
            {
                var stream = FileHelper.ToStream(artwork.ByteStream);
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(async () =>
                {
                    MainColor = await BitmapHelper.New(stream);
                    PlayPauseButton.Background = new SolidColorBrush(MainColor);
                }));
            }
            NowListMessage nowList;
            if (MessageService.TryParseMessage(e.Data, out nowList))
            {

                this.Dispatcher.RunAsync(CoreDispatcherPriority.High, new DispatchedHandler(() =>
                 {
                     NowListSource.Source = nowList.CurrentSongs;
                     CurrentPlayingList.ItemsSource = NowListSource.View;
                     NowListLoadingRing.IsActive = false;
                     NowListLoadingRing.Visibility = Visibility.Collapsed;
                     NowListLoadingText.Visibility = Visibility.Collapsed;
                     CurrentPlayingList.Visibility = Visibility.Visible;
                 }));
            }
            FullFileDetailsMessage detail;
            if (MessageService.TryParseMessage(e.Data, out detail))
            {
                updatedetail(detail);
            }
        }

        private void updatedetail(FullFileDetailsMessage detail)
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.High, new DispatchedHandler(() =>
             {
                 SongDetails.Update(CurrentSong, detail.BitRate, detail.Size, detail.MusicType);
                 ToolTip t1 = new ToolTip();
                 t1.Content = SongDetails.MainKey;
                 ToolTipService.SetToolTip(CurrentSongMainKey, t1);
                 ToolTip t2 = new ToolTip();
                 t2.Content = SongDetails.Album;
                 ToolTipService.SetToolTip(CurrentSongAlbum, t2);
                 ToolTip t3 = new ToolTip();
                 t3.Content = SongDetails.Title;
                 ToolTipService.SetToolTip(CurrentSongTitle, t3);
             }));
        }

        private void updateui()
        {
            NowTitle.Text = CurrentSong.Title;
            NowAlbum.Text = CurrentSong.Album;
            GenresDetailsConverter converter = new GenresDetailsConverter();
            NowDetails.Text = (string)converter.Convert(CurrentSong, null, null, null);
            var con = new ArtistsConverter();
            NowArtist.Text = (string)con.Convert(CurrentSong.Artists, null, true, null);
            var c = PositionSlider.ThumbToolTipValueConverter as ThumbToolTipConveter;
            c.sParmeter = CurrentSong.Duration.TotalSeconds;
            var d = new DurationValueConverter();
            TotalTimeBlock.Text = (string)d.Convert(CurrentSong.Duration, null, null, null);

        }

        private void ellipse_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            wtftimer.Cancel();
            PositionSlider.ValueChanged += PositionSlider_ValueChanged;
        }

        private void PositionSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            BackgroundMediaPlayer.Current.Position = TimeSpan.FromMilliseconds((((BackgroundMediaPlayer.Current.NaturalDuration.TotalMilliseconds) * PositionSlider.Value) / 100.0));
        }

        private void PositionSlider_Loaded(object sender, RoutedEventArgs e)
        {

            wtftimer = ThreadPoolTimer.CreatePeriodicTimer((source) =>
            {
                if (NowState == MediaPlayerState.Playing)
                {
                    this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        PositionSlider.Value = ((BackgroundMediaPlayer.Current.Position.TotalSeconds) / (BackgroundMediaPlayer.Current.NaturalDuration.TotalSeconds)) * 100.0;
                    });
                }
            },
            TimeSpan.FromSeconds(0.16));
        }

        private void HorizontalThumb_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            PositionSlider.ValueChanged -= PositionSlider_ValueChanged;
            wtftimer.Cancel();
            wtftimer = ThreadPoolTimer.CreatePeriodicTimer((source) =>
            {
                if (NowState == MediaPlayerState.Playing)
                {
                    this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        PositionSlider.Value = ((BackgroundMediaPlayer.Current.Position.TotalSeconds) / (BackgroundMediaPlayer.Current.NaturalDuration.TotalSeconds)) * 100.0;
                    });
                }
            },
            TimeSpan.FromSeconds(0.16));
        }

        private void OneStarButton_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case "OneStarButton": OneStar.Begin(); break;
                case "TwoStarButton": TwoStar.Begin(); break;
                case "ThreeStarButton": ThreeStar.Begin(); break;
                case "FourStarButton": FourStar.Begin(); break;
                case "FiveStarButton": FiveStar.Begin(); break;
            }
        }

        private void OneStarButton_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            switch (CurrentSong.Rating)
            {
                case 0: NoStar.Begin(); break;
                case 1: OneStarSet.Begin(); break;
                case 2: TwoStarSet.Begin(); break;
                case 3: ThreeStarSet.Begin(); break;
                case 4: FourStarSet.Begin(); break;
                case 5: FiveStarSet.Begin(); break;
            }
        }

        private void OneStarButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case "OneStarButton": OneStarSet.Begin(); CurrentSong.Rating = 1; break;
                case "TwoStarButton": TwoStarSet.Begin(); CurrentSong.Rating = 2; break;
                case "ThreeStarButton": ThreeStarSet.Begin(); CurrentSong.Rating = 3; break;
                case "FourStarButton": FourStarSet.Begin(); CurrentSong.Rating = 4; break;
                case "FiveStarButton": FiveStarSet.Begin(); CurrentSong.Rating = 5; break;
            }
        }

        private void LoveButton_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            LoveButtonOver.Begin();
        }

        private void LoveButton_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!Loved && !Broken)
            {
                LoveButtonNormal.Begin();
            }
            else if (Loved)
            {
                LoveButtonLove.Begin();
            }
            else if (Broken)
            {
                LoveButtonBreak.Begin();
            }
        }

        private void LoveButton_Click(object sender, RoutedEventArgs e)
        {
            Loved = !Loved;
            if (Loved)
            {
                LoveButtonLove.Begin();
            }
            else if (Broken)
            {
                LoveButtonBreak.Begin();
            }
        }

        internal async void updateartwork(byte[] artworkStream)
        {
            var stream = FileHelper.ToStream(artworkStream);
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(async () =>
            {
                MainColor = await BitmapHelper.New(stream);
                PlayPauseButton.Background = new SolidColorBrush(MainColor);
                ((Window.Current.Content as Frame).Content as MainPage).NotifyLrcPageArtworkChanged(PlayPauseButton.Foreground);
            }));
        }

        private void DeDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            ((Window.Current.Content as Frame).Content as MainPage).GoBack();
        }

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            ((Window.Current.Content as Frame).Content as MainPage).PlayPauseButtonOnLeft_Click(null, null);
            if (NowState == MediaPlayerState.Playing)
            {
                NowState = MediaPlayerState.Paused;
                var icon = new BitmapIcon();
                icon.UriSource = new Uri("ms-appx:///Assets/ButtonIcon/playsolid.png");
                PlayPauseButton.Content = icon;
                return;
            }

            if (NowState == MediaPlayerState.Paused || NowState == MediaPlayerState.Stopped)
            {
                NowState = MediaPlayerState.Playing;
                var icon = new BitmapIcon();
                icon.UriSource = new Uri("ms-appx:///Assets/ButtonIcon/pausesolid.png");
                PlayPauseButton.Content = icon;
                return;
            }

        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (NowState != MediaPlayerState.Stopped)
                ((Window.Current.Content as Frame).Content as MainPage).NextButton_Click(null, null);
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (NowState != MediaPlayerState.Stopped)
                ((Window.Current.Content as Frame).Content as MainPage).PreviousButton_Click(null, null);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            ((Window.Current.Content as Frame).Content as MainPage).StopButton_Click(null, null);
            NowState = MediaPlayerState.Stopped;
            var icon = new BitmapIcon();
            icon.UriSource = new Uri("ms-appx:///Assets/ButtonIcon/playsolid.png");
            PlayPauseButton.Content = icon;
        }

        private void ListButton_Click(object sender, RoutedEventArgs e)
        {
            NowListLoadingRing.IsActive = true;
            MessageService.SendMessageToBackground(new NeedNowListMessage());
        }

        private void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            ((Window.Current.Content as Frame).Content as MainPage).ShuffleButton_Click(null, null);
            NowMode = ((Window.Current.Content as Frame).Content as MainPage).NowMode;
            switch (NowMode)
            {
                case PlaybackMode.Normal:
                    ShuffleButton.IsChecked = true;
                    break;
                case PlaybackMode.Repeat:
                    ShuffleButton.IsChecked = true;
                    break;
                case PlaybackMode.RepeatSingle:
                    ShuffleButton.IsChecked = false;
                    break;
                case PlaybackMode.Shuffle:
                    ShuffleButton.IsChecked = false;
                    break;
                case PlaybackMode.ShuffleRepeat:
                    ShuffleButton.IsChecked = false;
                    break;
                default:
                    break;
            }
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            ((Window.Current.Content as Frame).Content as MainPage).RepeatButton_Checked(null, null);
            NowMode = ((Window.Current.Content as Frame).Content as MainPage).NowMode;
            switch (NowMode)
            {
                case PlaybackMode.Normal:
                    RepeatButton.IsChecked = true;
                    (RepeatButton.FindName("RepeatSymbol") as SymbolIcon).Symbol = Symbol.RepeatAll;
                    break;
                case PlaybackMode.Repeat:
                    RepeatButton.IsChecked = true;
                    (RepeatButton.FindName("RepeatSymbol") as SymbolIcon).Symbol = Symbol.RepeatOne;
                    break;
                case PlaybackMode.RepeatSingle:
                    RepeatButton.IsChecked = false;
                    (RepeatButton.FindName("RepeatSymbol") as SymbolIcon).Symbol = Symbol.RepeatAll;
                    break;
                case PlaybackMode.Shuffle:
                    RepeatButton.IsChecked = true;
                    break;
                case PlaybackMode.ShuffleRepeat:
                    RepeatButton.IsChecked = true;
                    (RepeatButton.FindName("RepeatSymbol") as SymbolIcon).Symbol = Symbol.RepeatOne;
                    ShuffleButton.IsChecked = false;
                    break;
                default:
                    break;
            }
        }

        private void FastMuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (VolumeSlider.Value > 0)
            {
                volume = VolumeSlider.Value;
                VolumeSlider.Value = 0;
            }
            else if (volume > 0)
            {
                VolumeSlider.Value = volume;
            }
        }

        private void VolumeSlider_Loaded(object sender, RoutedEventArgs e)
        {
            VolumeSlider.Value = BackgroundMediaPlayer.Current.Volume * 100;
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
        }

        private void VolumeSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            BackgroundMediaPlayer.Current.Volume = VolumeSlider.Value / 100.0;
            ((Window.Current.Content as Frame).Content as MainPage).VolumeSlider_ChangeValue(VolumeSlider.Value);
        }

        private void VolumeFlyout_Closed(object sender, object e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["Volume"] = VolumeSlider.Value;
        }

        private void Flyout_Closed(object sender, object e)
        {
            CurrentPlayingList.Visibility = Visibility.Collapsed;
            CurrentPlayingList.ItemsSource = null;
            NowListSource.Source = null;
            NowListLoadingRing.IsActive = false;
            NowListLoadingRing.Visibility = Visibility.Visible;
            NowListLoadingText.Visibility = Visibility.Visible;
        }

        private void MoreDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageService.SendMessageToBackground(new NeedFullFileDetailsMessage(CurrentSong.MainKey));
        }
    }
}
