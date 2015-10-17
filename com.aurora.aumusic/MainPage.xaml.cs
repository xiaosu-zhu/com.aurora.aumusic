using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace com.aurora.aumusic
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private static int MEDIA_ENDED_FLAG = 0;
        private static int MEDIA_PRESSED_FLAG = 0;
        SplitListView splitlistview;
        private static int BUTTON_CLICKED = 0;

        public MainPage()
        {
            this.InitializeComponent();
            //设置汉堡按钮控制
            //默认打开MymusicPage
            splitlistview = new SplitListView();
            SplitViewSources.Source = splitlistview;
            //MainFrame.Navigate(typeof(SettingsPage));
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
            if (MEDIA_ENDED_FLAG == 1)
            {
                AlbumFlowPage a = MainFrame.Content as AlbumFlowPage;
                await a.playbackctrl.PlayNext(this.PlaybackControl);
                MEDIA_ENDED_FLAG = 0;
            }
            MEDIA_PRESSED_FLAG = 0;
        }

        private async void SetMediaEnd(object sender, RoutedEventArgs e)
        {
            if (MEDIA_PRESSED_FLAG == 0)
            {
                AlbumFlowPage a = MainFrame.Content as AlbumFlowPage;
                await a.playbackctrl.PlayNext(this.PlaybackControl);
            }
            else
            {
                MEDIA_ENDED_FLAG = 1;
            }

        }

        private void ellipse_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            MEDIA_PRESSED_FLAG = 1;
        }

        private void PlaybackControl_MediaOpened(object sender, RoutedEventArgs e)
        {
            AlbumFlowPage a = MainFrame.Content as AlbumFlowPage;
            Song s = a.playbackctrl.NowPlaying();
            BitmapImage b = new BitmapImage(new Uri(s.ArtWork));
            PlayBackImage.Source = b;
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
                                MainFrame.Navigate(typeof(AlbumFlowPage), PlaybackControl); break;
                            }
                        }
                        MainFrame.Navigate(typeof(SettingsPage)); break;
                    case "Artists": MainFrame.Navigate(typeof(ArtistPage)); break;
                    case "Songs": MainFrame.Navigate(typeof(SongsPage)); break;
                    case "Song Lists": MainFrame.Navigate(typeof(ListPage)); break;
                }
            }
        }
    }
}
