using System;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Graphics.DirectX;
using Windows.Graphics.Effects;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;

using Windows.UI.Popups;

using Windows.Storage.Streams;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media;
using com.aurora.aumusic.shared;
using com.aurora.aumusic.shared.Songs;
using Windows.Media.Playback;
using com.aurora.aumusic.shared.MessageService;




//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace com.aurora.aumusic
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        PlaybackControl playbackControl;
        SplitListView splitlistview;
        private static int BUTTON_CLICKED = 0;
        PlayBack playBack = new PlayBack();
        //TextBlock TimeElapsedBlock;
        //TextBlock TimeTotalBlock;
        //Slider ProgressSlider;
        //Slider VolumeSlider;
        BitmapIcon vol_low = new BitmapIcon(), vol_mid = new BitmapIcon(), vol_mute = new BitmapIcon(), vol_high = new BitmapIcon(), vol_no = new BitmapIcon();
        AppBarButton volumrMuteButton;
        RenderTargetBitmap renderer = new RenderTargetBitmap();
        CanvasBitmap bitmap;
        byte[] RendererStream;
        ThreadPoolTimer wtftimer;

        public static int FrameWidth { get; private set; }
        public static int FrameHeight { get; private set; }
        public ICanvasImage RenderFinal { get; private set; }
        public MediaPlayerState NowState = MediaPlayerState.Stopped;

        public bool Frame_Updated = false;

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



        private async void HorizontalThumb_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            //if (playBack.NowPlaying() != null)
            //{
            //    if ((PlaybackControl.Position + TimeSpan.FromMilliseconds(500)) >= playBack.NowPlaying().Duration)
            //    {
            //        await playBack.PlayNext(PlaybackControl);
            //    }
            //    else
            //    {
            //        PlaybackControl.Play();
            //    }
            //}
            //PlaybackControl.MediaEnded += SetMediaEnd;
        }
        private async void SetMediaEnd(object sender, RoutedEventArgs e)
        {
            //AlbumFlowPage a = MainFrame.Content as AlbumFlowPage;
            //await playBack.PlayNext(this.PlaybackControl);
        }

        private void ellipse_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //PlaybackControl.MediaEnded -= SetMediaEnd;
        }

        private void PlaybackControl_MediaOpened(object sender, RoutedEventArgs e)
        {
            Song s = playBack.NowPlaying();
            TimeSpan ts = s.Duration;
            if (ts.Seconds >= 10)
            {
                string p = ts.Hours * 60 + ts.Minutes + ":" + ts.Seconds;
                TimeRemainingBlock.Text = p;
            }
            else
            {
                string p = ts.Hours * 60 + ts.Minutes + ":0" + ts.Seconds;
                TimeRemainingBlock.Text = p;
            }
            BitmapImage b = s != null ? new BitmapImage(new Uri(s.ArtWork)) : new BitmapImage(new Uri("ms-appx:///Assets/unknown.png"));
            PlayBackImage.Source = b;
            //ThumbToolTipConveter thumbConverter = (ThumbToolTipConveter)ProgressSlider.ThumbToolTipValueConverter;
            //thumbConverter.sParmeter = ts.TotalSeconds;
        }

        private void MenuList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView l = sender as ListView;
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
                switch (s.Title)
                {
                    case "AlbumFlow":
                        if (localSettings.Values.ContainsKey("FolderSettings"))
                        {
                            ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["FolderSettings"];
                            if (composite != null)
                            {
                                MainFrame.Navigate(typeof(AlbumFlowPage));
                                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
                                BackgroundMediaPlayer.Current.CurrentStateChanged += MediaPlayer_CurrentStateChanged;
                                break;
                            }
                        }
                        MainFrame.Navigate(typeof(SettingsPage)); l.SelectedIndex = -1; break;
                    case "Artists": MainFrame.Navigate(typeof(ArtistPage)); break;
                    case "Songs": MainFrame.Navigate(typeof(SongsPage)); break;
                    case "Song Lists": MainFrame.Navigate(typeof(ListPage)); break;
                }
            }
        }

        private async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            BackPlaybackChangedMessage stateChangedMessage;
            if (MessageService.TryParseMessage(e.Data, out stateChangedMessage))
            {
                // When foreground app is active change track based on background message
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // If playback stopped then clear the UI
                    if (stateChangedMessage.CurrentSong == null)
                    {
                        playbackControl.setPlaybackControlDefault();
                        return;
                    }
                    else
                        playbackControl.setPlaybackControl(stateChangedMessage.CurrentSong);
                    playbackControl.setPlaybackControl(stateChangedMessage.NowState);
                    NowState = stateChangedMessage.NowState;
                });
                return;
            }
        }

        private async void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (NowState == MediaPlayerState.Paused)
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    playbackControl.setPlaybackControl(MediaPlayerState.Paused);
                });

            }
            else if (NowState == MediaPlayerState.Stopped)
            {
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    playbackControl.setPlaybackControl(MediaPlayerState.Stopped);
                });

            }
        }

        private void ProgressSlider_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressSlider = sender as Slider;
        }

        private async void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (playBack.NowPlaying() != null)
            {
                //if (PlaybackControl.Position <= TimeSpan.FromSeconds((playBack.NowPlaying().Duration.TotalSeconds / 99)))
                //{
                //    await playBack.PlayPrevious(PlaybackControl);
                //}
                //else
                //{
                //    PlaybackControl.Position = TimeSpan.FromSeconds(0);
                //    PlaybackControl.Play();
                //}
            }
        }

        private async void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (playBack.NowPlaying() != null)
            {
                //await playBack.PlayNext(PlaybackControl);
            }
        }



        private void VolumeSlider_Loaded(object sender, RoutedEventArgs e)
        {
            VolumeSlider = sender as Slider;
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
        }

        private void VolumeFlyout_Closed(object sender, object e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["Volume"] = VolumeSlider.Value;
        }

        private void VolumeSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (((Slider)sender).Value == 0)
            {
                volumrMuteButton.Icon = vol_no;
            }
            else if (((Slider)sender).Value < 20)
            {
                volumrMuteButton.Icon = vol_mute;
            }
            else if (((Slider)sender).Value < 50)
            {
                volumrMuteButton.Icon = vol_low;
            }
            else if (((Slider)sender).Value < 80)
            {
                volumrMuteButton.Icon = vol_mid;
            }
            else volumrMuteButton.Icon = vol_high;
        }

        private void PlaybackControl_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
        }

        #region
        //animation of left-bottom Albumart
        private void PlayBackGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            PlayBackFore.Visibility = Visibility.Visible;
        }

        private void PlayBackGrid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            PlayBackFore.Visibility = Visibility.Collapsed;
        }
        #endregion
        private async void MainFrame_LayoutUpdated(object sender, object e)
        {

        }

        private void generate(ICanvasResourceCreator sender)
        {
            bitmap = CanvasBitmap.CreateFromBytes(sender, RendererStream, FrameWidth, FrameHeight, DirectXPixelFormat.B8G8R8A8UIntNormalized);
        }


        private ICanvasImage CropandBlur(CanvasBitmap bitmap)
        {
            //var crop = new CropEffect
            //{
            //    Source = bitmap
            //};
            //crop.SourceRectangle = new Rect(0, FrameHeight - 64, FrameWidth, 64);
            var blur = new GaussianBlurEffect
            {
                Source = bitmap
            };
            blur.BorderMode = EffectBorderMode.Hard;
            blur.BlurAmount = 8.0f;
            return blur;
        }

        private void MainFrame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
        }

        private void BlurLayer_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            //if (Frame_Updated)
            //{
            //    generate(args.DrawingSession);
            //    RenderFinal = CropandBlur(bitmap);
            //    Frame_Updated = false;
            //}
            //if (RenderFinal != null)
            //    args.DrawingSession.DrawImage(RenderFinal,0,-(FrameHeight-64));
        }

        private void VolumeMuteButton_Loaded(object sender, RoutedEventArgs e)
        {
            volumrMuteButton = sender as AppBarButton;
            //if (PlaybackControl.Volume == 0)
            //{
            //    volumrMuteButton.Icon = vol_no;
            //}
            //else if (PlaybackControl.Volume < 0.2)
            //{
            //    volumrMuteButton.Icon = vol_mute;
            //}
            //else if (PlaybackControl.Volume < 0.5)
            //{
            //    volumrMuteButton.Icon = vol_low;
            //}
            //else if (PlaybackControl.Volume < 0.8)
            //{
            //    volumrMuteButton.Icon = vol_mid;
            //}
            //else volumrMuteButton.Icon = vol_high;

        }

        private void PlayBackControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.ContainsKey("Volume"))
            {
                VolumeSlider.Value = (double)(localSettings.Values["Volume"]);
            }

            if (VolumeSlider.Value == 0)
            {
                volumrMuteButton.Icon = vol_no;
            }
            else if (VolumeSlider.Value < 20)
            {
                volumrMuteButton.Icon = vol_mute;
            }
            else if (VolumeSlider.Value < 50)
            {
                volumrMuteButton.Icon = vol_low;
            }
            else if (VolumeSlider.Value < 80)
            {
                volumrMuteButton.Icon = vol_mid;
            }
            else volumrMuteButton.Icon = vol_high;
            playBack.NotifyPlayBackEvent += PlayBack_NotifyPlayBackEvent;
            playbackControl = new PlaybackControl(PlayBackControl);
        }

        private void NowPlayingDetailsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            NowPlayingOut.Begin();
        }

        private void FastMuteButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PlayBack_NotifyPlayBackEvent(object sender, NotifyPlayBackEventArgs e)
        {
            if (wtftimer != null)
                wtftimer.Cancel();
            NowPlayingDetailsGrid.Visibility = Visibility.Visible;
            NowPlayingIn.Begin();
            wtftimer = ThreadPoolTimer.CreateTimer((source) =>
            {
                Dispatcher.RunAsync(
                           CoreDispatcherPriority.High,
                           () =>
                           {
                               NowPlayingOut.Begin();

                           });
                NowPlayingDetailsGrid.Visibility = Visibility.Collapsed;

            }, TimeSpan.FromSeconds(2));

        }
    }
}
