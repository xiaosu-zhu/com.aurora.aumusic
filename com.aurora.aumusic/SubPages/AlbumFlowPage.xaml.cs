using System.Diagnostics;
using System.Linq;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Threading;
using System;
using Windows.Foundation;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Input;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media.Animation;
using Windows.System.Threading;
using Windows.ApplicationModel;

namespace com.aurora.aumusic
{
    public sealed partial class AlbumFlowPage : Page
    {
        PlaybackPack _pageParameters;
        AlbumEnum Albums = new AlbumEnum();
        AlbumItem DetailedAlbum;
        bool isInitialed = false;
        private ScrollViewer DetailsScrollViewer;
        public double HeaderHeight { get; private set; }
        public double MaxScrollHeight { get; private set; }
        public Task<List<Song>> GeneratefavListTask { get; private set; }
        public Grid AlbumDetailsHeader { get; private set; }
        public Image AlbumArtWork { get; private set; }
        public TextBlock AlbumTitle { get; private set; }
        public TextBlock AlbumDetailsBlock { get; private set; }

        public AlbumFlowPage()
        {
            this.InitializeComponent();

            AlbumFlowResources.Source = Albums.Albums;
            var view = ApplicationView.GetForCurrentView();
            ApplicationViewTitleBar titleBar = view.TitleBar;
            if (titleBar != null)
            {
                titleBar.BackgroundColor = Color.FromArgb(255, 240, 240, 240);
                titleBar.ButtonBackgroundColor = Color.FromArgb(255, 240, 240, 240);

            }
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            AlbumFlowZoom.IsZoomedInViewActive = false;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            if (isInitialed)
            {
                var view = ApplicationView.GetForCurrentView();
                ApplicationViewTitleBar titleBar = view.TitleBar;
                if (titleBar != null)
                {
                    titleBar.BackgroundColor = Color.FromArgb(255, 240, 240, 240);
                    titleBar.ButtonBackgroundColor = Color.FromArgb(255, 240, 240, 240);
                }

                //if (Albums.AlbumList.Count >= 64)
                //{
                //    Albums.Albums.Clear();
                //}
                return;
            }
            _pageParameters = e.Parameter as PlaybackPack;
        }

        private async void WaitingBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (isInitialed)
            {
                //if (Albums.AlbumList.Count >= 64)
                //{
                //    await Task.Delay(1);
                //    int i = 0;
                //    foreach (var item in Albums.AlbumList)
                //    {
                //        i++;
                //        if (i % 64 == 0)
                //            await Task.Delay(1);
                //        Albums.Albums.Add(item);
                //    }
                //    GC.Collect();
                //}
                WaitingBar.Visibility = Visibility.Collapsed;
                WaitingBar.IsIndeterminate = false;
                return;
            }
            var v = await Albums.RestoreAlbums();
            switch (v)
            {
                case RefreshState.NeedCreate: await Albums.FirstCreate(); break;
                case RefreshState.NeedRefresh: await Albums.Refresh(); break;
                case RefreshState.Normal: break;
            }
            WaitingBar.Visibility = Visibility.Collapsed;
            WaitingBar.IsIndeterminate = false;
            isInitialed = true;
            Albums.AlbumList = Albums.Albums.ToList();
            Application.Current.Suspending += SaveLists;
        }

        private void SaveLists(object sender, SuspendingEventArgs e)
        {
            ShuffleList shuffleList = new ShuffleList(Albums.AlbumList);
            shuffleList.SaveShuffleList(shuffleList.GenerateNewList(20));
            ShuffleList.SaveFavouriteList(shuffleList.GenerateFavouriteList());

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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            AlbumItem album = ((Button)sender).DataContext as AlbumItem;
            await this._pageParameters.PlaybackControl.Play(album.Songs, _pageParameters.Media);
        }


        private void RelativePanel_PointerReleased_1(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            DetailedAlbum = ((RelativePanel)sender).DataContext as AlbumItem;
            ZoomInInitial();
        }

        private void Zoom_BackRequested(object sender, BackRequestedEventArgs e)
        {
            AlbumFlowZoom.IsZoomedInViewActive = false;
            var view = ApplicationView.GetForCurrentView();
            ApplicationViewTitleBar titleBar = view.TitleBar;
            titleBar.BackgroundColor = Color.FromArgb(255, 240, 240, 240);
            titleBar.ButtonBackgroundColor = Color.FromArgb(255, 240, 240, 240);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= Zoom_BackRequested;
            DetailedAlbum = null;
        }

        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            DetailsScrollViewer = sender as ScrollViewer;
        }



        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _pageParameters.PlaybackControl.Clear();
            _pageParameters.PlaybackControl.addNew(DetailedAlbum);
            await _pageParameters.PlaybackControl.Play(_pageParameters.Media);
        }

        private void RelativePanel_PointerEntered_1(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            RelativePanel r = sender as RelativePanel;
            Button b = ((Button)r.Children[3]);
            b.Visibility = Visibility.Visible;
        }

        private void RelativePanel_PointerExited_1(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            RelativePanel r = sender as RelativePanel;
            Button b = ((Button)r.Children[3]);
            b.Visibility = Visibility.Collapsed;
        }

        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            Song s = ((Button)sender).DataContext as Song;
            int index = DetailedAlbum.Songs.IndexOf(s);
            _pageParameters.PlaybackControl.Clear();
            _pageParameters.PlaybackControl.addNew(DetailedAlbum);
            await _pageParameters.PlaybackControl.Play(index, _pageParameters.Media);
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
            HeaderHeight = 424;
            AlbumDetailsHeader.Background = new SolidColorBrush(DetailedAlbum.Palette);
            AlbumArtWork.Source = new BitmapImage(new Uri(DetailedAlbum.AlbumArtWork));
            AlbumTitle.Foreground = new SolidColorBrush(DetailedAlbum.TextMainColor);
            AlbumDetailsBlock.Foreground = new SolidColorBrush(DetailedAlbum.TextSubColor);
            AlbumDetailsConverter albumDetials = new AlbumDetailsConverter();
            string s = (string)albumDetials.Convert(DetailedAlbum, null, null, null);
            AlbumDetailsBlock.Text = s;
            AlbumTitle.Text = DetailedAlbum.AlbumName;
            var view = ApplicationView.GetForCurrentView();
            ApplicationViewTitleBar titleBar = view.TitleBar;
            titleBar.BackgroundColor = DetailedAlbum.Palette;
            titleBar.ButtonBackgroundColor = DetailedAlbum.Palette;
            SystemNavigationManager.GetForCurrentView().BackRequested += Zoom_BackRequested;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            //bool completed = false;
            //completed = TimeTask(TimeSpan.FromMilliseconds(10), completed);
            //RootGrid.Background = new SolidColorBrush(DetailedAlbum.Palette);
        }

        //private bool TimeTask(TimeSpan delay, bool completed)
        //{
        //    ThreadPoolTimer DelayTimer = ThreadPoolTimer.CreateTimer(
        //                       async (source) =>
        //                       {
        //                           await
        //                   Dispatcher.RunAsync(
        //                        CoreDispatcherPriority.High,
        //                        () =>
        //                        {
        //                            DetailsViewbox.Margin = new Thickness(0, DetailsScrollViewer.VerticalOffset, 0, 0);
        //                        });

        //                           completed = true;
        //                       },
        //                            delay,
        //                     async (source) =>
        //                     {
        //                         await

        //                      Dispatcher.RunAsync(
        //                 CoreDispatcherPriority.High,
        //                 () =>
        //                 {

        //                     if (completed)
        //                     {
        //                         if (AlbumFlowZoom.IsZoomedInViewActive == false)
        //                             return;
        //                         completed = TimeTask(delay, completed);
        //                     }
        //                     else
        //                     {
        //                     }

        //                 });
        //                     });
        //    return completed;
        //}
        private async void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            List<Song> shuffleList = new List<Song>();

            //shuffleList.AddRange(await ShuffleList.RestoreFavouriteList());

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
