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
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Windows.UI.Xaml.Media;
using com.aurora.aumusic.shared;
using com.aurora.aumusic.shared.Songs;
using Windows.Media.Playback;
using com.aurora.aumusic.shared.MessageService;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ViewManagement;
using Windows.UI;




//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace com.aurora.aumusic
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public CurrentTheme Theme = new CurrentTheme();
        PlaybackControl playbackControl;
        SplitListView splitlistview;
        private static int BUTTON_CLICKED = 0;
        PlayBack playBack = new PlayBack();
        BitmapIcon vol_low = new BitmapIcon(), vol_mid = new BitmapIcon(), vol_mute = new BitmapIcon(), vol_high = new BitmapIcon(), vol_no = new BitmapIcon();
        ThreadPoolTimer wtftimer;

        public static int FrameWidth { get; private set; }
        public static int FrameHeight { get; private set; }
        public ICanvasImage RenderFinal { get; private set; }
        public MediaPlayerState NowState = MediaPlayerState.Stopped;
        private PlaybackMode _playbackmode = PlaybackMode.Normal;
        public PlaybackMode NowMode
        {
            get
            {
                return _playbackmode;
            }
            private set
            {
                this._playbackmode = value;
                MessageService.SendMessageToBackground(new ForePlaybackChangedMessage(_playbackmode));
            }
        }

        public byte[] ArtworkStream { get; private set; }

        public bool NavigatedtoDetailPages = false;

        public SongModel CurrentSong = null;

        public bool Frame_Updated = false;
        private NowPlayingHub nowPlayingHub;
        private double volume = 0;
        private ThreadPoolTimer periodtimer;

        public MainPage()
        {
            this.InitializeComponent();
            //设置汉堡按钮控制
            //默认打开MymusicPage
            splitlistview = new SplitListView();
            SplitViewSources.Source = splitlistview;
            //MainFrame.Navigate(typeof(SettingsPage));
            vol_mid.UriSource = new Uri("ms-appx:///Assets/ButtonIcon/volume-mid.png");
            vol_low.UriSource = new Uri("ms-appx:///Assets/ButtonIcon/volume-low.png");
            vol_mute.UriSource = new Uri("ms-appx:///Assets/ButtonIcon/volume-mute.png");
            vol_high.UriSource = new Uri("ms-appx:///Assets/ButtonIcon/volume-high.png");
            vol_no.UriSource = new Uri("ms-appx:///Assets/ButtonIcon/volume-no.png");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var str = (string)ApplicationSettingsHelper.ReadSettingsValue("SpacePause");
            if (str == null || str == "true")
            {
                this.KeyUp += Root_KeyUp;
            }
            App.ResetTitleBar();
        }

        public void UnBindSpace()
        {
            this.KeyUp -= Root_KeyUp;
        }

        private void Menubtn_Click(object sender, RoutedEventArgs e)
        {
            //菜单按钮点击定义
            Menudrawer.IsPaneOpen = !Menudrawer.IsPaneOpen;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (BUTTON_CLICKED == 0)
            {
                BUTTON_CLICKED = 1;
                this.MainFrame.Navigate(typeof(SettingsPage));
                SettingsButton.IsEnabled = false;
                MenuList.SelectedItem = null;
            }
        }

        internal void ShowRefresh()
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                RefreshPanel.Visibility = Visibility.Visible;
                RefreshPanelIn.Begin();
                periodtimer = ThreadPoolTimer.CreatePeriodicTimer((t) =>
                {
                    this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        RotateTrans.Angle += 6;
                    });
                }, TimeSpan.FromMilliseconds(16));
            });
        }

        private void ellipse_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            wtftimer.Cancel();
            ProgressSlider.ValueChanged += ProgressSlider_ValueChanged;
        }

        private void ProgressSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (ProgressSlider.Value == 100)
            {
                return;
            }
            BackgroundMediaPlayer.Current.Position = TimeSpan.FromMilliseconds((((BackgroundMediaPlayer.Current.NaturalDuration.TotalMilliseconds) * ProgressSlider.Value) / 100.0));
        }

        private void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView l = sender as ListView;
            if (NowPlayingDetailsGrid.Visibility == Visibility.Collapsed)
            {
                GoBack();
            }
            foreach (var item in splitlistview)
            {
                item.visibility = Visibility.Collapsed;
            }
            if (l.SelectedIndex != -1)
            {
                BUTTON_CLICKED = 0;
                SettingsButton.IsEnabled = true;
                Splitlist s = l.SelectedItem as Splitlist;
                s.visibility = Visibility.Visible;
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                switch (l.SelectedIndex)
                {
                    case 0:
                        if (localSettings.Values.ContainsKey("FolderSettings"))
                        {
                            ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["FolderSettings"];
                            if (composite != null && (int)composite["FolderCount"] > 0)
                            {
                                MainFrame.Navigate(typeof(AlbumFlowPage), this);
                                break;
                            }
                        }
                       MainFrame.Navigate(typeof(StartPage)); break;
                    case 1: MainFrame.Navigate(typeof(ArtistPage)); break;
                    case 2:
                        MainFrame.Navigate(typeof(SongsPage));
                        break;
                    case 3: MainFrame.Navigate(typeof(ListPage)); break;
                }
            }
        }

        public void AddMediaHandler()
        {
            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;
        }

        private void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (NowState)
                {
                    case MediaPlayerState.Closed:
                        break;
                    case MediaPlayerState.Opening:
                        break;
                    case MediaPlayerState.Buffering:
                        break;
                    case MediaPlayerState.Playing:
                        playbackControl.setPlaybackControl(MediaPlayerState.Playing);
                        NowPlayingIn.Begin();
                        CommandBarOut.Begin();
                        MediaControlsCommandBar.PointerEntered += NowPlayingDetailsGrid_PointerEntered;
                        MediaControlsCommandBar.PointerExited += NowPlayingDetailsGrid_PointerExited;
                        break;
                    case MediaPlayerState.Paused:
                        playbackControl.setPlaybackControl(MediaPlayerState.Paused);
                        NowPlayingOut.Begin();
                        CommandBarIn.Begin();
                        MediaControlsCommandBar.PointerEntered -= NowPlayingDetailsGrid_PointerEntered;
                        MediaControlsCommandBar.PointerExited -= NowPlayingDetailsGrid_PointerExited;
                        break;
                    case MediaPlayerState.Stopped:
                        playbackControl.setPlaybackControl(MediaPlayerState.Stopped);
                        NowPlayingOut.Begin();
                        CommandBarIn.Begin();
                        MediaControlsCommandBar.PointerEntered -= NowPlayingDetailsGrid_PointerEntered;
                        MediaControlsCommandBar.PointerExited -= NowPlayingDetailsGrid_PointerExited;
                        break;
                    default:
                        break;
                }
            });

        }

        internal void BindSpace()
        {
            this.KeyUp += Root_KeyUp;
        }

        internal void FinishCreate()
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                CreateRing.Visibility = Visibility.Collapsed;
                App.ResetTitleBar();
                RefreshPanelOut.Begin();
                if (periodtimer != null)
                {
                    periodtimer.Cancel();
                }
                await Task.Delay(700);
                RefreshPanel.Visibility = Visibility.Collapsed;
            });

        }

        private async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            BackgroundConfirmFilesMessage confirming;
            if (MessageService.TryParseMessage(e.Data, out confirming))
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    LoadingFilesBar.IsIndeterminate = false;
                    LoadingFilesBar.Visibility = Visibility.Collapsed;
                });
            }
            BackPlaybackChangedMessage stateChangedMessage;
            if (MessageService.TryParseMessage(e.Data, out stateChangedMessage))
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {

                    // If playback stopped then clear the UI
                    if (stateChangedMessage.CurrentSong == null)
                    {
                        playbackControl.setPlaybackControlDefault();

                    }
                    else
                    {
                        playbackControl.setPlaybackControl(stateChangedMessage.CurrentSong);
                        this.CurrentSong = stateChangedMessage.CurrentSong;
                        nowPlayingHub.Set(stateChangedMessage.CurrentSong);
                        if (NowLrcFrame.Content != null)
                        {
                            NowLrcFrame.Navigate(typeof(LrcPage), CurrentSong);
                        }
                    }
                    NowState = stateChangedMessage.NowState;
                    BackgroundMediaPlayer.Current.Volume = VolumeSlider.Value / 100.0;
                    var converter = ProgressSlider.ThumbToolTipValueConverter as ThumbToolTipConveter;
                    converter.sParameter = stateChangedMessage.CurrentSong.Duration.TotalSeconds;
                    ArtworkLoadingRing.IsActive = true;
                    ArtworkLoadingRing.Visibility = Visibility.Visible;
                });
            }
            FileNotFindMessage notfind;
            if (MessageService.TryParseMessage(e.Data, out notfind))
            {
                this.Dispatcher.RunAsync(CoreDispatcherPriority.High, new DispatchedHandler(() =>
                 {
                     FileErrPanel.Visibility = Visibility.Visible;
                     ErrPanelIn.Begin();
                     ThreadPoolTimer.CreateTimer((a) =>
                    {
                        this.Dispatcher.RunAsync(CoreDispatcherPriority.High, new DispatchedHandler(async () =>
                        {
                            ErrPanelOut.Begin();
                            await Task.Delay(256);
                            FileErrPanel.Visibility = Visibility.Collapsed;
                        }));
                    }, TimeSpan.FromSeconds(3));
                 }));
            }
            UpdateArtworkMessage updateartwork;
            if (MessageService.TryParseMessage(e.Data, out updateartwork))
            {
                IRandomAccessStream stream;
                stream = await FileHelper.ToRandomAccessStream(updateartwork.ByteStream);
                this.ArtworkStream = updateartwork.ByteStream;
                this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ArtworkLoadingRing.IsActive = false;
                    ArtworkLoadingRing.Visibility = Visibility.Collapsed;
                    BitmapImage image = new BitmapImage();
                    image.SetSource(stream);
                    PlayBackImage.Source = image;
                    PlayBackImage_ImageOpened(stream);
                });
            }
        }

        internal void UpdateProgress(double v, double count)
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                var cole = new DoubleCollection();
                var progress = v + count;
                CreateProgressText.Text = string.Format("{0}%", (progress * 100).ToString("0"));
                progress *= 161.7924;
                cole.Add(progress);
                cole.Add(1470);
                ProgressRoll.StrokeDashArray = cole;

            });
        }

        internal async void GoBack()
        {
            PlayBackGrid.PointerEntered += PlayBackGrid_PointerEntered;
            PlayBackGrid.PointerExited += PlayBackGrid_PointerExited;

            var renderer = new RenderTargetBitmap();
            await renderer.RenderAsync(NowLrcFrame);
            var rendererr = new RenderTargetBitmap();
            await rendererr.RenderAsync(NowCtrlFrame);
            NowLrcFrame.Navigate(typeof(CachePage), renderer);
            NowCtrlFrame.Navigate(typeof(CachePage), rendererr);
            var ani = MainFrameIn.Children[0] as DoubleAnimation;
            ani.To = Root.ActualHeight;
            ani = PlaybackControlGridOut.Children[0] as DoubleAnimation;
            ani.From = Root.ActualHeight;
            PlaybackControlGridUnset.Begin();
            PlaybackControlGridOut.Begin();
            MainFrameIn.Begin();

            NavigatedtoDetailPages = false;

            await Task.Delay(700);
            NowPlayingDetailsGrid.Visibility = Visibility.Visible;
            MediaTransportControls_Timeline_Grid.Visibility = Visibility.Visible;
            TimeRemainingBlock.Visibility = Visibility.Visible;
            TimePastBlock.Visibility = Visibility.Visible;
            LeftPanel.Visibility = Visibility.Visible;
            MediaControlsCommandBar.Visibility = Visibility.Visible;
            if (MenuList.SelectedIndex == -1)
                MainFrame.Navigate(typeof(SettingsPage));
            switch (MenuList.SelectedIndex)
            {
                case 0: MainFrame.Navigate(typeof(AlbumFlowPage)); break;
                case 1: MainFrame.Navigate(typeof(ArtistPage)); break;
                case 2: MainFrame.Navigate(typeof(SongsPage)); break;
                case 3: MainFrame.Navigate(typeof(ListPage)); break;
            }
            MainFrameSet.Begin();
            NowLrcFrame.Visibility = Visibility.Collapsed;
            NowCtrlFrame.Visibility = Visibility.Collapsed;
            NowLrcFrame.Content = null;
            NowCtrlFrame.Content = null;
            Root.LayoutUpdated += Root_LayoutUpdated;
        }

        private void ProgressSlider_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressSlider.IsEnabled = false;
            wtftimer = ThreadPoolTimer.CreatePeriodicTimer((source) =>
            {
                if (NowState == MediaPlayerState.Playing)
                {
                    this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        try
                        {
                            ProgressSlider.Value = ((BackgroundMediaPlayer.Current.Position.TotalSeconds) / (BackgroundMediaPlayer.Current.NaturalDuration.TotalSeconds)) * 100.0;
                        }
                        catch (Exception)
                        {

                        }
                    });
                }
            },
            TimeSpan.FromSeconds(0.16));
        }

        public void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (NowState != MediaPlayerState.Stopped)
                MessageService.SendMessageToBackground(new ForePlaybackChangedMessage(PlaybackState.Previous));
        }

        public void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (NowState != MediaPlayerState.Stopped)
                MessageService.SendMessageToBackground(new ForePlaybackChangedMessage(PlaybackState.Next));
        }



        private void VolumeSlider_Loaded(object sender, RoutedEventArgs e)
        {
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
        }

        private void VolumeFlyout_Closed(object sender, object e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["Volume"] = VolumeSlider.Value;
        }

        public void VolumeSlider_ChangeValue(double value)
        {
            VolumeSlider.Value = value;
        }

        private void VolumeSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (((Slider)sender).Value == 0)
            {
                VolumeMuteButton.Icon = vol_no;
            }
            else if (((Slider)sender).Value < 20)
            {
                VolumeMuteButton.Icon = vol_mute;
            }
            else if (((Slider)sender).Value < 50)
            {
                VolumeMuteButton.Icon = vol_low;
            }
            else if (((Slider)sender).Value < 80)
            {
                VolumeMuteButton.Icon = vol_mid;
            }
            else VolumeMuteButton.Icon = vol_high;
            try
            {
                BackgroundMediaPlayer.Current.Volume = VolumeSlider.Value / 100.0;
            }
            catch (Exception)
            {

            }
        }

        #region
        //animation of left-bottom Albumart
        private void PlayBackGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AlbumArtWorkIn.Begin();
        }

        private void PlayBackGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            AlbumArtWorkOut.Begin();
        }
        #endregion

        private void PlayBackFore_Loaded(object sender, RoutedEventArgs e)
        {
            AlbumArtWorkOut.Begin();
        }

        private void HorizontalThumb_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ProgressSlider.ValueChanged -= ProgressSlider_ValueChanged;
            wtftimer.Cancel();
            wtftimer = ThreadPoolTimer.CreatePeriodicTimer((source) =>
            {
                if (NowState == MediaPlayerState.Playing)
                {
                    this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        ProgressSlider.Value = ((BackgroundMediaPlayer.Current.Position.TotalSeconds) / (BackgroundMediaPlayer.Current.NaturalDuration.TotalSeconds)) * 100.0;
                    });
                }
            },
            TimeSpan.FromSeconds(0.16));
        }

        public void FirstCreate()
        {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
             {
                 var view = ApplicationView.GetForCurrentView();
                 ApplicationViewTitleBar titleBar = view.TitleBar;
                 if (titleBar != null)
                 {
                     titleBar.BackgroundColor = Color.FromArgb(255, 0, 116, 206);
                     titleBar.ButtonBackgroundColor = Color.FromArgb(255, 0, 116, 206);
                 }
                 CreateRing.Visibility = Visibility.Visible;
                 var cole = new DoubleCollection();
                 cole.Add(0);
                 cole.Add(1470);
                 ProgressRoll.StrokeDashArray = cole;
                 CreateProgressText.Text = string.Format("{0}%", 0);
             });

        }

        public void PlayPauseButtonOnLeft_Click(object sender, RoutedEventArgs e)
        {
            switch (NowState)
            {
                case MediaPlayerState.Closed:
                    break;
                case MediaPlayerState.Opening:
                    break;
                case MediaPlayerState.Buffering:
                    break;
                case MediaPlayerState.Playing:
                    MessageService.SendMessageToBackground(new ForePlaybackChangedMessage(PlaybackState.Paused));
                    NowState = MediaPlayerState.Paused;
                    break;
                case MediaPlayerState.Paused:
                    MessageService.SendMessageToBackground(new ForePlaybackChangedMessage(PlaybackState.Playing));
                    NowState = MediaPlayerState.Playing;
                    break;
                case MediaPlayerState.Stopped:
                    MessageService.SendMessageToBackground(new ForePlaybackChangedMessage(PlaybackState.Playing));
                    NowState = MediaPlayerState.Playing;
                    break;
                default:
                    break;
            }

        }

        private async void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            await AccesstoNowPage();

        }

        private async Task AccesstoNowPage()
        {
            if (CurrentSong != null && NavigatedtoDetailPages == false)
            {
                AlbumArtWorkOut.Begin();
                Root.LayoutUpdated -= Root_LayoutUpdated;
                var renderer = new RenderTargetBitmap();
                await renderer.RenderAsync(MainFrame);
                MainFrame.Navigate(typeof(CachePage), renderer);
                PlayBackGrid.PointerEntered -= PlayBackGrid_PointerEntered;
                PlayBackGrid.PointerExited -= PlayBackGrid_PointerExited;
                NowLrcFrame.Visibility = Visibility.Visible;
                NowLrcFrame.Navigate(typeof(LrcPage), CurrentSong);
                NowCtrlFrame.Visibility = Visibility.Visible;
                NowCtrlFrame.Navigate(typeof(NowPage), CurrentSong);
                if (ArtworkStream != null)
                    (NowCtrlFrame.Content as NowPage).updateartwork(ArtworkStream);
                NavigatedtoDetailPages = true;
                NowPlayingDetailsGrid.Visibility = Visibility.Collapsed;
                MediaTransportControls_Timeline_Grid.Visibility = Visibility.Collapsed;
                TimeRemainingBlock.Visibility = Visibility.Collapsed;
                TimePastBlock.Visibility = Visibility.Collapsed;
                LeftPanel.Visibility = Visibility.Collapsed;
                MediaControlsCommandBar.Visibility = Visibility.Collapsed;
                MainFrameUnset.Begin();
                var ani = MainFrameOut.Children[0] as DoubleAnimation;
                ani.From = Root.ActualHeight;
                ani = PlaybackControlGridIn.Children[0] as DoubleAnimation;
                ani.To = Root.ActualHeight;
                PlaybackControlGridIn.Begin();
                MainFrameOut.Begin();

                await Task.Delay(700);
                PlaybackControlGridSet.Begin();
            }
        }

        public void ShuffleButton_Click(object sender, RoutedEventArgs e)
        {
            switch (NowMode)
            {
                case PlaybackMode.Normal:
                    NowMode = PlaybackMode.Shuffle;
                    ShuffleButton.IsChecked = true;
                    break;
                case PlaybackMode.Repeat:
                    NowMode = PlaybackMode.ShuffleRepeat;
                    ShuffleButton.IsChecked = true;
                    break;
                case PlaybackMode.RepeatSingle:
                    ShuffleButton.IsChecked = false;
                    break;
                case PlaybackMode.Shuffle:
                    NowMode = PlaybackMode.Normal;
                    ShuffleButton.IsChecked = false;
                    break;
                case PlaybackMode.ShuffleRepeat:
                    NowMode = PlaybackMode.Repeat;
                    ShuffleButton.IsChecked = false;
                    break;
                default:
                    break;
            }
        }

        public void StopButton_Click(object sender, RoutedEventArgs e)
        {
            MessageService.SendMessageToBackground(new ForePlaybackChangedMessage(PlaybackState.Stopped));
            NowState = MediaPlayerState.Stopped;
            CurrentSong = null;
        }

        public void RepeatButton_Checked(object sender, RoutedEventArgs e)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            TextBlock text = (TextBlock)ToolTipService.GetToolTip(RepeatButton);
            switch (NowMode)
            {
                case PlaybackMode.Normal:
                    RepeatButton.IsChecked = true;
                    (RepeatButton.FindName("RepeatSymbol") as SymbolIcon).Symbol = Symbol.RepeatAll;
                    NowMode = PlaybackMode.Repeat;
                    text.Text = loader.GetString("RepeatAllTip.Text");
                    break;
                case PlaybackMode.Repeat:
                    RepeatButton.IsChecked = true;
                    (RepeatButton.FindName("RepeatSymbol") as SymbolIcon).Symbol = Symbol.RepeatOne;
                    NowMode = PlaybackMode.RepeatSingle;
                    text.Text = loader.GetString("RepeatOnceTip.Text");
                    break;
                case PlaybackMode.RepeatSingle:
                    RepeatButton.IsChecked = false;
                    (RepeatButton.FindName("RepeatSymbol") as SymbolIcon).Symbol = Symbol.RepeatAll;
                    NowMode = PlaybackMode.Normal;
                    text.Text = loader.GetString("RepeatButtonTip.Text");
                    break;
                case PlaybackMode.Shuffle:
                    (RepeatButton.FindName("RepeatSymbol") as SymbolIcon).Symbol = Symbol.RepeatAll;
                    RepeatButton.IsChecked = true;
                    NowMode = PlaybackMode.ShuffleRepeat;
                    text.Text = loader.GetString("RepeatAllTip.Text");
                    break;
                case PlaybackMode.ShuffleRepeat:
                    RepeatButton.IsChecked = true;
                    (RepeatButton.FindName("RepeatSymbol") as SymbolIcon).Symbol = Symbol.RepeatOne;
                    ShuffleButton.IsChecked = false;
                    NowMode = PlaybackMode.RepeatSingle;
                    text.Text = loader.GetString("RepeatOnceTip.Text");
                    break;
                default:
                    break;
            }
        }

        private void NowPlayingDetailsGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            NowPlayingOut.Begin();
            CommandBarIn.Begin();
        }

        private void NowPlayingDetailsGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            NowPlayingIn.Begin();
            CommandBarOut.Begin();
        }

        private async void PlayBackGrid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            await AccesstoNowPage();

        }

        private void Root_LayoutUpdated(object sender, object e)
        {
            MainFrame.Height = Root.ActualHeight;
        }

        private async void PlayBackImage_ImageOpened(IRandomAccessStream stream)
        {
            if (CurrentSong != null)
            {
                {
                    var device = new CanvasDevice();
                    var bitmap = await CanvasBitmap.LoadAsync(device, stream);

                    var renderer = new CanvasRenderTarget(device,
                                                          bitmap.SizeInPixels.Width,
                                                          bitmap.SizeInPixels.Height, bitmap.Dpi);

                    using (var ds = renderer.CreateDrawingSession())
                    {
                        var blur = new GaussianBlurEffect
                        {
                            Source = bitmap
                        };
                        blur.BlurAmount = 16.0f;
                        blur.BorderMode = EffectBorderMode.Hard;
                        ds.DrawImage(blur);
                    }

                    stream.Seek(0);
                    await renderer.SaveAsync(stream, CanvasBitmapFileFormat.Png);
                    stream.Seek(0);
                    BitmapImage image = new BitmapImage();
                    image.SetSource(stream);
                    BackgroundBlur.Source = image;
                    renderer = null;
                    bitmap = null;
                    device = null;
                    GC.Collect();
                }
            }
        }

        private void Root_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Space)
            {
                this.PlayPauseButtonOnLeft_Click(null, null);
            }
        }

        internal void NavigatetoSettings()
        {
            if (BUTTON_CLICKED == 0)
            {
                BUTTON_CLICKED = 1;
                this.MainFrame.Navigate(typeof(SettingsPage));
                SettingsButton.IsEnabled = false;
                MenuList.SelectedItem = null;
            }
        }

        private void PlayBackControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("Volume"))
            {
                VolumeSlider.Value = (double)(localSettings.Values["Volume"]);
            }
            else
            {
                VolumeSlider.Value = 100;
            }

            if (VolumeSlider.Value == 0)
            {
                VolumeMuteButton.Icon = vol_no;
            }
            else if (VolumeSlider.Value < 20)
            {
                VolumeMuteButton.Icon = vol_mute;
            }
            else if (VolumeSlider.Value < 50)
            {
                VolumeMuteButton.Icon = vol_low;
            }
            else if (VolumeSlider.Value < 80)
            {
                VolumeMuteButton.Icon = vol_mid;
            }
            else VolumeMuteButton.Icon = vol_high;
            playbackControl = new PlaybackControl(PlayBackControl);
        }

        private void NowPlayingDetailsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            NowPlayingOut.Begin();
            nowPlayingHub = new NowPlayingHub(sender as Grid);
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
    }
}
