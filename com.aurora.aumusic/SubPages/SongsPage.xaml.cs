using System.Diagnostics;
using Windows.UI.Xaml.Controls;

namespace com.aurora.aumusic
{
    public sealed partial class SongsPage : Page
    {
        SongsEnum Songs = new SongsEnum();
        public SongsPage()
        {
            this.InitializeComponent();
            SongListReosurces.Source = Songs.Songs;
        }

        private async void Progress_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Songs.GetSongsWithProgress();
            WaitingBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void SongList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Item = SongList.SelectedItem;
        }

        private void hehButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Songs.SaveSongstoStorage();
        }

        private void hahButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            WaitingBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Songs.RestoreSongsfromStorage();
        }
    }
}
