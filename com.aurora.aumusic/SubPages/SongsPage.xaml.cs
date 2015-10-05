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
            Songs.GetSongs(await Songs.RefreshList());
            WaitingRing.IsActive = false;
            WaitingRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void SongList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Item = SongList.SelectedItem;
        }
    }
}
