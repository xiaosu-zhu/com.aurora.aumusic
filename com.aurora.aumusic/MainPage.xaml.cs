using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace com.aurora.aumusic
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private int ListViewCount = 0;
        public MainPage()
        {
            this.InitializeComponent();
            //设置汉堡按钮控制
            Window.Current.SetTitleBar(Titlepanel);
            //默认打开MymusicPage
            MainFrame.Navigate(typeof(AlbumFlowPage));
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
                                MainFrame.Navigate(typeof(AlbumFlowPage)); break;
                            }
                        }
                        MainFrame.Navigate(typeof(SettingsPage)); break;
                    case 1: MainFrame.Navigate(typeof(NowPage)); break;
                    case 2: MainFrame.Navigate(typeof(ArtistPage)); break;
                    case 3: MainFrame.Navigate(typeof(SongsPage)); break;
                    case 4: MainFrame.Navigate(typeof(ListPage)); break;

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
    }
}
