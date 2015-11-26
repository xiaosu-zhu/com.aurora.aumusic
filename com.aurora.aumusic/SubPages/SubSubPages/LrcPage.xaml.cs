using Windows.UI.Xaml.Controls;
using Kfstorm.LrcParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using com.aurora.aumusic.shared;
using com.aurora.aumusic.shared.Lrc;
using Windows.UI.Xaml.Navigation;
using com.aurora.aumusic.shared.Songs;
using System.Threading;
using Windows.System.Threading;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.Media.Playback;
using Windows.UI.Xaml.Media.Imaging;

namespace com.aurora.aumusic
{
    public sealed partial class LrcPage : Page
    {
        Song CurrentSong;
        AutoResetEvent trigger = new AutoResetEvent(false);
        ILrcFile lyric = null;


        public LrcPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
                this.CurrentSong = new Song(e.Parameter as SongModel);
        }


        public async Task genLrc()
        {
            string result = null;
            try
            {
                result = await LrcHelper.Fetch(await LrcHelper.isLrcExist(CurrentSong), CurrentSong);
            }
            catch (System.Net.WebException)
            {
                this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
                {
                    ErrText.Text = "没有网络连接";
                }));

            }

            if (result == null)
            {
                lyric = null;
                return;
            }
            else
            {
                var stream = await FileHelper.ReadFileasString(result);
                try
                {
                    lyric = LrcFile.FromText(stream);
                }
                catch (FormatException e)
                {
                    var strings = e.Message.Split('\'');
                    string s = strings[1].Substring(strings[1].IndexOf(':') + 1);
                    s = s.TrimEnd('0');
                    int j = stream.IndexOf(s);
                    int start, end;
                    for (int i = j; ; i--)
                    {
                        if (stream[i] == '\n')
                        {
                            start = i;
                            for (j = stream.IndexOf(s); ; j++)
                            {
                                if (stream[j] == '\n')
                                {
                                    end = j;
                                    break;
                                }
                            }
                            break;
                        }

                    }
                    StringBuilder sb = new StringBuilder(stream.Substring(0, start));
                    sb.Append(stream.Substring(end));
                    s = sb.ToString();
                    await FileHelper.SaveFile(s, result);
                    lyric = LrcFile.FromText(s);
                }

                trigger.Set();
            }
        }

        private async void WaitingRing_Loaded(object sender, RoutedEventArgs e)
        {
            IAsyncAction asyncAction = ThreadPool.RunAsync(
                         async (workItem) =>
                         {
                             await FetchLrc();
                             await this.Dispatcher.RunAsync(
                                                              CoreDispatcherPriority.High,
                                                              new DispatchedHandler(() =>
                                                              {
                                                                  if (lyric == null)
                                                                      return;
                                                                  LyricSource.Source = lyric.Lyrics;
                                                                  LyricView.ItemsSource = LyricSource.View;
                                                                  WaitingRing.IsActive = false;
                                                                  WaitingPanel.Visibility = Visibility.Collapsed;
                                                              }));
                             if (lyric != null)
                             {
                                 ThreadPoolTimer timer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
                                 {
                                     var now = BackgroundMediaPlayer.Current.Position;
                                     await this.Dispatcher.RunAsync(CoreDispatcherPriority.High, new DispatchedHandler(async () =>
                                     {

                                         LyricView.SelectedIndex = lyric.Lyrics.IndexOf(lyric.BeforeOrAt(now));
                                         await LyricView.ScrollToIndex(LyricView.SelectedIndex);
                                     }));

                                 }, TimeSpan.FromSeconds(1));

                             }
                         });

            await Task.Run(async () =>
            {
                bool result = trigger.WaitOne(15000);
                if (result == false)
                {
                    asyncAction.Cancel();
                    asyncAction.Close();
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.High, new DispatchedHandler(() =>
                    {
                        ErrPanel.Visibility = Visibility.Visible;
                        WaitingRing.IsActive = false;
                        WaitingPanel.Visibility = Visibility.Collapsed;
                        LyricView.IsEnabled = false;
                        LyricView.Visibility = Visibility.Collapsed;

                    }));
                }
            });
        }

        private async Task FetchLrc()
        {
            var uri = CurrentSong.Title + "-" + CurrentSong.Artists[0] + "-" + CurrentSong.Album + ".lrc";
            try
            {
                lyric = LrcFile.FromText(await FileHelper.ReadFileasString(uri));
                trigger.Set();
            }
            catch (Exception)
            {
                await genLrc();
            }
        }

        private void LyricView_LayoutUpdated(object sender, object e)
        {
            DynamicFooter.Height = LyricView.ActualHeight;
        }
    }
}
