using System.Linq;
using Windows.UI.Xaml.Controls;

namespace com.aurora.aumusic
{
    public sealed partial class AlbumFlowView : Page
    {
        AlbumEnum Albums = new AlbumEnum();
        public AlbumFlowView()
        {
            this.InitializeComponent();
            AlbumFlowResources.Source = Albums.Albums;
        }

        private async void WaitingBar_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Albums.getAlbumList();
            WaitingBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            WaitingBar.IsIndeterminate = false;
        }
    }
}
