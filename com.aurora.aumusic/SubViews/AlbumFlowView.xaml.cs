using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
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
            WaitingBar.Visibility = Visibility.Collapsed;
            WaitingBar.IsIndeterminate = false;
        }

        private void AlbumsFlowControls_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine("HEIHEI");
        }

        private void RelativePanel_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
        }

        
    }
}
