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
        public MainPage()
        {
            this.InitializeComponent();
            Window.Current.SetTitleBar(Titlepanel);
            MainFrame.Navigate(typeof(com.aurora.aumusic.MymusicPage));
        }

        private void Menubtn_Click(object sender, RoutedEventArgs e)
        {
            Menudrawer.IsPaneOpen = !Menudrawer.IsPaneOpen;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int x = MenuList.SelectedIndex;
            switch(x)
            {
                case 0: MainFrame.Navigate(typeof(com.aurora.aumusic.MymusicPage)); break;
                case 1: MainFrame.Navigate(typeof(NowPage)); break;
                case 2: MainFrame.Navigate(typeof(ListPage)); break;
                case 3: MainFrame.Navigate(typeof(SettingsPage)); break;

            }
        }
    }
}
