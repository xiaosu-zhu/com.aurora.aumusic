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
using System.Linq;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Threading;
using System;
using Windows.Foundation;
using System.Collections.Generic;
using Windows.UI.ViewManagement;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Windows.System.Threading;
using Windows.ApplicationModel;
using com.aurora.aumusic.shared.Albums;
using com.aurora.aumusic.shared.Songs;
using com.aurora.aumusic.shared;
using com.aurora.aumusic.backgroundtask;
using com.aurora.aumusic.shared.MessageService;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace com.aurora.aumusic
{
    /// <summary>
    /// This is the proccessing code of the AlbumFlowpage
    /// </summary>
    public sealed partial class AlbumFlowPage : Page
    {
        CurrentTheme Theme = ((Window.Current.Content as Frame).Content as MainPage).Theme;
        AlbumEnum Albums = new AlbumEnum();
        AlbumItem DetailedAlbum;
        List<Song> AllSongs = new List<Song>();
        bool IsInitialed = false;
        private ScrollViewer DetailsScrollViewer;
        public Grid AlbumDetailsHeader { get; private set; }
        public Image AlbumArtWork { get; private set; }
        public TextBlock AlbumTitle { get; private set; }
        public TextBlock AlbumDetailsBlock { get; private set; }
        public ScrollViewer AlbumFlowScroller { get; private set; }
        public Rectangle ShuffleHeader { get; private set; }
        public bool IsShuffleListInitialed { get; private set; }
        public TextBlock SongsDetailsBlock { get; private set; }
        public TextBlock GenresDetailsBlock { get; private set; }
        private AutoResetEvent backgroundAudioTaskStarted;
        private bool isMyBackgroundTaskRunning = false;
        private bool IsMyBackgroundTaskRunning
        {
            get
            {
                return isMyBackgroundTaskRunning;
            }
        }

        public ThreadPoolTimer DelayTimer { get; private set; }
        public ThreadPoolTimer PeriodicTimer { get; private set; }

        public BackgroundTaskState BackgroundState = BackgroundTaskState.Stopped;

        private bool[] ShuffleArtworkState = new bool[4];//"true" means the first image is Showed.
        List<string> ShuffleArts = new List<string>();

        public AlbumFlowPage()
        {
            this.InitializeComponent();
            AlbumFlowResources.Source = Albums.albumList;
            this.NavigationCacheMode = NavigationCacheMode.Required;
            AlbumFlowZoom.IsZoomedInViewActive = false;
            backgroundAudioTaskStarted = new AutoResetEvent(false);
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            App.ResetTitleBar();
            var str = ApplicationSettingsHelper.ReadSettingsValue("isCreated");
            if (str == null)
            {
                ((Window.Current.Content as Frame).Content as MainPage).FirstCreate();
            }
            str = ApplicationSettingsHelper.ReadResetSettingsValue("NewAdded");
            if(str != null)
            {
                //notify to refresh
            }
            AlbumFlowZoom.IsZoomedInViewActive = false;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            SystemNavigationManager.GetForCurrentView().BackRequested -= this.Zoom_BackRequested;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

        }
        private void AddMediaPlayerEventHandlers()
        {
            BackgroundMediaPlayer.MessageReceivedFromBackground += this.BackgroundMediaPlayer_MessageReceivedFromBackground;
            ((Window.Current.Content as Frame).Content as MainPage).AddMediaHandler();
        }

        private async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            RefreshStateMessage refresh;
            if (MessageService.TryParseMessage(e.Data, out refresh))
            {
                if (refresh.Refresh == RefreshState.NeedRefresh)
                {
                    Albums.notifyrefresh += Albums_notifyrefresh;
                    await Albums.Refresh();
                }
                Albums.notifyrefresh -= Albums_notifyrefresh;
            }

            BackgroundTaskStateChangedMessage backgroundTaskMessage;
            if (MessageService.TryParseMessage(e.Data, out backgroundTaskMessage))
            {
                // StartBackgroundAudioTask is waiting for this signal to know when the task is up and running
                // and ready to receive messages
                if (backgroundTaskMessage.TaskState == BackgroundTaskState.Running && BackgroundState != BackgroundTaskState.Running)
                {
                    backgroundAudioTaskStarted.Set();
                    BackgroundState = BackgroundTaskState.Running;
                }
                return;
            }
        }

        private void Albums_notifyrefresh(object sender, NotifyRefreshEventArgs e)
        {
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            this.Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                Albums.RefreshtoList(e.item);
            });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
        }

        private void StartBackgroundAudioTask()
        {
            AddMediaPlayerEventHandlers();
            if (!IsMyBackgroundTaskRunning)
            {
                var startResult = this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                bool result = backgroundAudioTaskStarted.WaitOne(100000);
                                //Send message to initiate playback
                                if (result == true)
                                {
                                    if (Albums.albumList.Count > 0)
                                        MessageService.SendMessageToBackground(new UpdatePlaybackMessage(Albums.albumList.ToSongModelList()));
                                }
                                else
                                {
                                    throw new Exception("Background Audio Task didn't start in expected time");
                                }
                            });
                isMyBackgroundTaskRunning = true;
            }
            else
            {
                if (Albums.albumList.Count > 0)
                    MessageService.SendMessageToBackground(new UpdatePlaybackMessage(Albums.albumList.ToSongModelList()));
            }
        }

        internal object GetAllSongs()
        {
            return this.AllSongs;
        }

        private async void WaitingBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsInitialed)
            {
                HideBar(WaitingBar);
                return;
            }
            RefreshState v = RefreshState.Normal;
            v = Albums.RestoreAlbums();
            Albums.CopytoAlbumList();
            Albums.progresschanged += Albums_progresschanged;
            switch (v)
            {
                case RefreshState.NeedCreate: await Albums.FirstCreate(); break;
                case RefreshState.NeedRefresh: await Albums.Refresh(); break;
                case RefreshState.Normal: break;
            }
            HideBar(WaitingBar);
            ((Window.Current.Content as Frame).Content as MainPage).FinishCreate();
            IsInitialed = true;
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == nameof(BackgroundAudio))
                {
                    isMyBackgroundTaskRunning = true;
                    break;
                }
            }
            StartBackgroundAudioTask();
            Application.Current.Suspending += SaveLists;
            TimeSpan period = TimeSpan.FromSeconds(5);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            ThreadPool.RunAsync((work) =>
             {
                 foreach (var album in Albums.albums)
                 {
                     foreach (var song in album.Songs)
                     {
                         this.AllSongs.Add(song);
                     }
                     SongsPage.AllSongs = this.AllSongs;
                     ArtistPage.AllSongs = this.Albums.albumList.ToList();
                     ListPage.AllSongs = this.AllSongs;
                 }
             });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            PeriodicTimer = ThreadPoolTimer.CreateTimer((source) =>
            {
                if (!IsShuffleListInitialed)
                {
                    ShuffleList shuffleList = new ShuffleList(Albums.albumList.ToList());
                    var list = shuffleList.GenerateNewList(ShuffleList.FAV_LIST_CAPACITY);
                    list = ShuffleList.ShuffleIt(list);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    Dispatcher.RunAsync(CoreDispatcherPriority.High,
async () =>
                    {
                        ShuffleListResources.Source = list;
                        await ShowShuffleArtwork(list);
                        HideBar(FavWaitingBar);
                    });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                }
            }, period);
        }

        private void Albums_progresschanged(object sender, AlbumProgressChangedEventArgs e)
        {
            ((Window.Current.Content as Frame).Content as MainPage).UpdateProgress(e.CurrentPercent, e.TotalPercent);
        }

        private void SaveLists(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            if (IsMyBackgroundTaskRunning)
            {
                // Stop handling player events immediately
                RemoveMediaPlayerEventHandlers();

                // Tell the background task the foreground is suspended
                MessageService.SendMessageToBackground(new AppStateChangedMessage(AppState.Suspended));
            }

            // Persist that the foreground app is suspended
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, AppState.Suspended.ToString());
            ShuffleList shuffleList = new ShuffleList(Albums.albumList.ToList());
            shuffleList.SaveShuffleList(shuffleList.GenerateNewList(ShuffleList.FAV_LIST_CAPACITY));
            ShuffleList.SaveFavouriteList(shuffleList.GenerateFavouriteList());
            deferral.Complete();
        }

        private void RemoveMediaPlayerEventHandlers()
        {
            BackgroundMediaPlayer.MessageReceivedFromBackground -= this.BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        private void RelativePanel_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ScrollViewer sv = ((RelativePanel)sender).Children[1] as ScrollViewer;
            if (sv == null)
                return;
            sv.ChangeView(0, 48, 1);
        }

        private void RelativePanel_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ScrollViewer sv = ((RelativePanel)sender).Children[1] as ScrollViewer;
            if (sv == null)
                return;
            sv.ChangeView(0, 0, 1);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AlbumItem album = ((Button)sender).DataContext as AlbumItem;
            MessageService.SendMessageToBackground(new ForePlaybackChangedMessage(PlaybackState.Playing, album.ToSongModelList()));
            //await this._pageParameters.PlaybackControl.Play(album.Songs, _pageParameters.Media);
        }


        private void RelativePanel_PointerReleased_1(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            DetailedAlbum = ((RelativePanel)sender).DataContext as AlbumItem;
            ZoomInInitial();
        }

        private void Zoom_BackRequested(object sender, BackRequestedEventArgs e)
        {
            AlbumFlowZoom.IsZoomedInViewActive = false;
            App.ResetTitleBar();
            SystemNavigationManager.GetForCurrentView().BackRequested -= Zoom_BackRequested;
            DetailedAlbum = null;
            e.Handled = true;
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            DetailsScrollViewer = sender as ScrollViewer;
        }



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MessageService.SendMessageToBackground(new ForePlaybackChangedMessage(PlaybackState.Playing, DetailedAlbum.ToSongModelList()));
            //_pageParameters.PlaybackControl.Clear();
            //_pageParameters.PlaybackControl.addNew(DetailedAlbum);
            //await _pageParameters.PlaybackControl.Play(_pageParameters.Media);
        }

        private void RelativePanel_PointerEntered_1(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            RelativePanel r = sender as RelativePanel;
            Button b = ((Button)r.Children[4]);
            b.Visibility = Visibility.Visible;
        }

        private void RelativePanel_PointerExited_1(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            RelativePanel r = sender as RelativePanel;
            Button b = ((Button)r.Children[4]);
            b.Visibility = Visibility.Collapsed;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            MessageService.SendMessageToBackground(new ForePlaybackChangedMessage(PlaybackState.Playing, DetailedAlbum.ToSongModelList(), new SongModel(((sender as Button).DataContext as Song))));
            // _pageParameters.PlaybackControl.Clear();
            //_pageParameters.PlaybackControl.addNew(DetailedAlbum);
            // await _pageParameters.PlaybackControl.Play(index, _pageParameters.Media);
        }



        private void Rectangle_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            DetailedAlbum = ((((sender as Rectangle).Parent as Grid).Parent as ScrollViewer).Parent as RelativePanel).DataContext as AlbumItem;
            ZoomInInitial();
            AlbumFlowZoom.IsZoomedInViewActive = true;
        }

        private void ZoomInInitial()
        {
            AlbumSongsResources.Source = DetailedAlbum.Songs;
            AlbumDetailsHeader.Background = new SolidColorBrush(DetailedAlbum.Palette);
            AlbumArtWork.Source = new BitmapImage(new Uri(DetailedAlbum.AlbumArtWork));
            var brush = new SolidColorBrush(DetailedAlbum.TextMainColor);
            AlbumTitle.Foreground = brush;
            SongsDetailsBlock.Foreground = brush;
            brush = new SolidColorBrush(DetailedAlbum.TextSubColor);
            AlbumDetailsBlock.Foreground = brush;
            GenresDetailsBlock.Foreground = brush;
            AlbumDetailsConverter albumDetials = new AlbumDetailsConverter();
            string s = (string)albumDetials.Convert(DetailedAlbum, null, null, null);
            AlbumDetailsBlock.Text = s;
            SongsDetailsConverter songsDetails = new SongsDetailsConverter();
            s = (string)songsDetails.Convert(DetailedAlbum, null, null, null);
            SongsDetailsBlock.Text = s;
            GenresDetailsConverter genresDetails = new GenresDetailsConverter();
            s = (string)genresDetails.Convert(DetailedAlbum, null, null, null);
            GenresDetailsBlock.Text = s;
            AlbumTitle.Text = DetailedAlbum.AlbumName;
            var view = ApplicationView.GetForCurrentView();
            ApplicationViewTitleBar titleBar = view.TitleBar;
            titleBar.BackgroundColor = DetailedAlbum.Palette;
            titleBar.ButtonBackgroundColor = DetailedAlbum.Palette;
            SystemNavigationManager.GetForCurrentView().BackRequested += Zoom_BackRequested;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            DetailsScrollViewer.ChangeView(0, 0, 1);
        }

        private void AlbumDetailsHeader_Loaded(object sender, RoutedEventArgs e)
        {
            AlbumDetailsHeader = sender as Grid;
        }

        private void AlbumArtWork_Loaded(object sender, RoutedEventArgs e)
        {
            AlbumArtWork = sender as Image;
        }

        private void AlbumTitle_Loaded(object sender, RoutedEventArgs e)
        {
            AlbumTitle = sender as TextBlock;
        }

        private void AlbumDetailsBlock_Loaded(object sender, RoutedEventArgs e)
        {
            AlbumDetailsBlock = sender as TextBlock;
        }
        private void ScrollViewer_Loaded_1(object sender, RoutedEventArgs e)
        {
            AlbumFlowScroller = sender as ScrollViewer;

        }

        private async void FavListView_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsInitialed)
                return;
            List<Song> shuffleList = new List<Song>();
            var s = await ShuffleList.RestoreFavouriteList();
            if (s != null)
            {
                shuffleList.AddRange(s);
            }
            var v = ShuffleList.RestoreShuffleList();
            if (v != null)
            {
                shuffleList.AddRange(v);
            }
            if (shuffleList.Count >= ShuffleList.FAV_LIST_CAPACITY)
            {
                List<Song> final;
                final = ShuffleList.ShuffleIt(shuffleList);
                ShuffleListResources.Source = final;
                ArtworkOutStoryboardAll.Begin();
                await ShowShuffleArtwork(final);
                IsShuffleListInitialed = true;
                HideBar(FavWaitingBar);
                Random r = new Random(Guid.NewGuid().GetHashCode());
                TimeSpan delay = TimeSpan.FromSeconds(r.Next(15) + 3);
                bool completed = false;
                completed = PlayShuffleArtwork(delay, completed);
            }
        }

        private bool PlayShuffleArtwork(TimeSpan delay, bool completed)
        {
            DelayTimer = ThreadPoolTimer.CreateTimer(
                (source) =>
                {
                    bool[] bools = ShuffleArtworkState.ToArray();
                    Random t = new Random(Guid.NewGuid().GetHashCode());
                    int count = t.Next(4);
                    while (count % 2 == 0)
                    {
                        count = t.Next(4);
                    }
                    for (int i = 0; i <= count; i++)
                    {
                        int m = t.Next(4);
                        ShuffleArtworkState[m] = !ShuffleArtworkState[m];
                    }

#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    Dispatcher.RunAsync(
                                            CoreDispatcherPriority.High,
                                            () =>
                                            {
                                                for (int i = 0; i < bools.Length; i++)
                                                {
                                                    if (bools[i] ^ ShuffleArtworkState[i])
                                                    {
                                                        ReverseShuffleArtwork(ShuffleArtworkState[i], i, ShuffleArts);
                                                    }
                                                }
                                            });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法

                    completed = true;
                },
                delay,
                (source) =>
                {
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    Dispatcher.RunAsync(
            CoreDispatcherPriority.High,
            () =>
            {
                // 
                // UI components can be accessed within this scope.
                // 

                if (completed)
                {
                    Random r = new Random(Guid.NewGuid().GetHashCode());
                    TimeSpan d = TimeSpan.FromSeconds(r.Next(7) + 3);
                    bool c = false;
                    completed = PlayShuffleArtwork(d, c);
                }
                else
                {
                }

            });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                });

            return completed;
        }


        private void ReverseShuffleArtwork(bool item, int j, List<string> arts)
        {
            var imagej = (Image)FavListView.FindName("ShuffleArtwork" + j.ToString());
            var imagejj = (Image)FavListView.FindName("ShuffleArtwork" + j.ToString() + j.ToString());
            int i = int.Parse(arts[0]);
            switch (item)
            {
                case true: ShowForeArtWork(imagej, arts, j, i); break;
                case false: ShowBackArtWork(imagejj, arts, j, i); break;
            }
        }

        private void ShowForeArtWork(Image imagej, List<string> arts, int j, int i)
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            imagej.Source = new BitmapImage(new Uri(arts[i]));
            i = (i + 1 == arts.Count) ? 1 : i + 1;
            arts[0] = i.ToString();
            switch (j)
            {
                case 1: ArtworkInStoryboard0.Begin(); break;
                case 2: ArtworkInStoryboard1.Begin(); break;
                case 3: ArtworkInStoryboard2.Begin(); break;
                case 4: ArtworkInStoryboard3.Begin(); break;
            }
        }

        private void ShowBackArtWork(Image imagejj, List<string> arts, int j, int i)
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            imagejj.Source = new BitmapImage(new Uri(arts[i]));
            i = (i + 1 == arts.Count) ? 1 : i + 1;
            arts[0] = i.ToString();
            switch (j)
            {
                case 1: ArtworkOutStoryboard0.Begin(); break;
                case 2: ArtworkOutStoryboard1.Begin(); break;
                case 3: ArtworkOutStoryboard2.Begin(); break;
                case 4: ArtworkOutStoryboard3.Begin(); break;
            }
        }

        private void HideBar(ProgressBar bar)
        {
            bar.Visibility = Visibility.Collapsed;
            bar.IsIndeterminate = false;
        }


        private async Task<List<string>> ShowShuffleArtwork(List<Song> final)
        {
            ShuffleArts.Clear();
            if (FavListView != null)
            {
                if (final != null && final.Count > 0)
                {
                    var image1 = (Image)FavListView.FindName("ShuffleArtwork0");
                    var image2 = (Image)FavListView.FindName("ShuffleArtwork1");
                    var image3 = (Image)FavListView.FindName("ShuffleArtwork2");
                    var image4 = (Image)FavListView.FindName("ShuffleArtwork3");
                    Image[] img = new Image[] { image1, image2, image3, image4 };
                    List<string> arts = new List<string>();
                    await Task.Run(() =>
                    {
                        foreach (var item in final)
                        {
                            if (arts.Contains(item.ArtWork))
                                continue;
                            arts.Add(item.ArtWork);
                        }
                    });
                    ShuffleArts.Add(1.ToString());
                    ShuffleArts.AddRange(arts.GetRange(0, arts.Count));
                    Random r = new Random(Guid.NewGuid().GetHashCode());
                    for (int j = 0; j < img.Length; j++)
                    {
                        if (arts.Count == 0)
                            break;
                        int m = r.Next(arts.Count);
                        img[j].Source = new BitmapImage(new Uri(arts[m]));
                        arts.RemoveAt(m);
                    }
                    ShuffleArtworkState = new bool[4] { true, true, true, true };
                    await Task.Delay(500);
                    ArtworkInStoryboardAll.Begin();
                }
            }
            return null;
        }

        private void RelativePanel_PointerEntered_2(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            RelativePanel r = sender as RelativePanel;
            Button b = ((Button)r.Children[3]);
            b.Visibility = Visibility.Visible;
        }

        private void RelativePanel_PointerExited_2(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            RelativePanel r = sender as RelativePanel;
            Button b = ((Button)r.Children[3]);
            b.Visibility = Visibility.Collapsed;
        }

        private void ShufflePlayButton_Click(object sender, RoutedEventArgs e)
        {
            SongModel song = new SongModel((Song)(sender as Button).DataContext);
            var list = new List<SongModel>();
            foreach (var item in (List<Song>)ShuffleListResources.Source)
            {
                list.Add(new SongModel(item));
            }
            MessageService.SendMessageToBackground(new ForePlaybackChangedMessage((PlaybackState.Playing), list, song));
        }

        private void FavListView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void SongsDetailsBlock_Loaded(object sender, RoutedEventArgs e)
        {
            SongsDetailsBlock = sender as TextBlock;
        }


        private void GenresDetailsBlock_Loaded(object sender, RoutedEventArgs e)
        {
            GenresDetailsBlock = sender as TextBlock;
        }

        private void ChangePathButton_Click(object sender, RoutedEventArgs e)
        {
            ((Window.Current.Content as Frame).Content as MainPage).NavigatetoSettings();
        }
    }



    public sealed class AlbumFlowPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            //横向瀑布流

            //三组流长度记录
            KeyValuePair<double, int>[] flowLength = new KeyValuePair<double, int>[3];
            foreach (int index in Enumerable.Range(0, 3))
            {
                flowLength[index] = new KeyValuePair<double, int>(0.0, index);
            }

            //每组长度为总长度1/3
            double flowWidth = availableSize.Width / 3;

            //子控件宽为组宽，长无限制
            Size childMeasureSize = new Size(flowWidth, double.PositiveInfinity);

            //子控件遍历计算长度
            foreach (UIElement childElement in Children)
            {
                childElement.Measure(childMeasureSize);
                Size childSize = childElement.DesiredSize;
                //得到子控件长
                double childLength = childSize.Height;
                //暂存最短流长度
                var tempPair = flowLength[0];
                //最短流长度重新计算
                flowLength[0] = new KeyValuePair<double, int>(tempPair.Key + childLength, tempPair.Value);
                //重新按流长度排列键值对        这里以Key 的值作为排列依据，flowWidth[0]为Key最小的键值对，P可替换为任意字母
                flowLength = flowLength.OrderBy(P => P.Key).ToArray();
            }
            //返回 长：最长流的长；宽：传入的宽
            return new Size(availableSize.Width, flowLength.Last().Key);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            KeyValuePair<double, int>[] flowLength = new KeyValuePair<double, int>[3];
            double flowWidth = finalSize.Width / 3;
            double[] xWidth = new double[3];
            foreach (int index in Enumerable.Range(0, 3))
            {
                flowLength[index] = new KeyValuePair<double, int>(0.0, index);
                xWidth[index] = index * flowWidth;
            }

            foreach (UIElement childElem in Children)
            {
                // 获取子控件期望大小
                Size elemSize = childElem.DesiredSize;
                double elemLength = elemSize.Height;

                //得到最短流长度
                var pair = flowLength[0];
                double chosenFlowHeight = pair.Key;
                int chosenFlowIdx = pair.Value;

                // 设置即将放置的控件坐标
                Point pt = new Point(xWidth[chosenFlowIdx], chosenFlowHeight);

                // 调用Arrange进行子控件布局。并让子控件利用上整个流的长度。
                childElem.Arrange(new Rect(pt, new Size(flowWidth, elemSize.Height)));

                // 重新计算最短流。
                flowLength[0] = new KeyValuePair<double, int>(chosenFlowHeight + elemLength, chosenFlowIdx);
                flowLength = flowLength.OrderBy(p => p.Key).ToArray();
            }
            return finalSize;
        }
    }

}
