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

namespace com.aurora.aumusic
{
    public sealed partial class AlbumFlowPage : Page
    {
        PlaybackPack _pageParameters;
        AlbumEnum Albums = new AlbumEnum();
        AlbumItem SelectedAlbum;
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
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _pageParameters = e.Parameter as PlaybackPack;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }

        private async void WaitingBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(await Albums.getAlbumList()))
            {
                await Albums.FirstCreate();
            }

            WaitingBar.Visibility = Visibility.Collapsed;
            WaitingBar.IsIndeterminate = false;
            BitmapHelper p = new BitmapHelper();
        }

        private void RelativePanel_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ScrollViewer sv = ((RelativePanel)sender).Children[1] as ScrollViewer;
            sv.ChangeView(0, 48, 1);
        }

        private void RelativePanel_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ScrollViewer sv = ((RelativePanel)sender).Children[1] as ScrollViewer;
            sv.ChangeView(0, 0, 1);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            AlbumItem album = ((Button)sender).DataContext as AlbumItem;
            await this._pageParameters.PlaybackControl.Play(album.Songs, _pageParameters.Media);
        }

        private void AlbumsFlowControls_ItemClick(object sender, ItemClickEventArgs e)
        {
            PlaybackPack p = new PlaybackPack();
            p.Album = SelectedAlbum;
            p.Media = this._pageParameters.Media;
            p.PlaybackControl = this._pageParameters.PlaybackControl;
            p.States = PLAYBACK_STATES.SingleAlbum;
            ((Frame)((AlbumFlowPage)((RelativePanel)((Grid)((ListView)sender).Parent).Parent).Parent).Parent).Navigate(typeof(AlbumDetails), p);
        }

        private void RelativePanel_PointerReleased_1(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            SelectedAlbum = ((RelativePanel)sender).DataContext as AlbumItem;
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
