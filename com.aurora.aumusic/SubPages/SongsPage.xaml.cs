using System.Diagnostics;
using Windows.UI.Xaml.Controls;

namespace com.aurora.aumusic
{
    public sealed partial class SongsPage : Page
    {
        public SongsPage()
        {
            this.InitializeComponent();
        }

        private void Progress_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //await Songs.RestoreSongsWithProgress();
        }

        private void SongList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           // var Item = SongList.SelectedItem;
        }

        private void hehButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
           // Songs.SaveSongstoStorage();
        }

        private void hahButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
        }
    }
}
