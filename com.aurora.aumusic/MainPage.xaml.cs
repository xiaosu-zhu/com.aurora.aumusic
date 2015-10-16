using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace com.aurora.aumusic
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private int ListViewCount = 0;
        private static int MEDIA_ENDED_FLAG = 0;
        private static int MEDIA_PRESSED_FLAG = 0;
        public MainPage()
        {
            this.InitializeComponent();
            //设置汉堡按钮控制
            //默认打开MymusicPage
            MainFrame.Navigate(typeof(SettingsPage));
        }

        private void Menubtn_Click(object sender, RoutedEventArgs e)
        {
            //菜单按钮点击定义
            Menudrawer.IsPaneOpen = !Menudrawer.IsPaneOpen;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

            //菜单项目点击定义
            int x = MenuList.SelectedIndex;
            if (x != ListViewCount)
            {
                SettingsButton.IsEnabled = true;
                switch (x)
                {
                    //页面跳转
                    case 0:
                        if (localSettings.Values.ContainsKey("FolderSettings"))
                        {
                            ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["FolderSettings"];
                            if (composite != null)
                            {
                                MainFrame.Navigate(typeof(AlbumFlowPage), PlaybackControl); break;
                            }
                        }
                        MainFrame.Navigate(typeof(SettingsPage)); break;
                    case 1: MainFrame.Navigate(typeof(ArtistPage)); break;
                    case 2: MainFrame.Navigate(typeof(SongsPage)); break;
                    case 3: MainFrame.Navigate(typeof(ListPage)); break;

                }
                ListViewCount = x;
                Menudrawer.IsPaneOpen = false;
            }

        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            {
                MainFrame.Navigate(typeof(SettingsPage));
                SettingsButton.IsEnabled = false;
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
    }
}
