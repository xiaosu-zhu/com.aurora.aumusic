using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace com.aurora.aumusic
{
    public sealed partial class AlbumDetails : Page
    {
        AlbumItem _pageParameters;
        public AlbumDetails()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //We are going to cast the property Parameter of NavigationEventArgs object
            //into PageWithParametersConfiguration.
            //PageWithParametersConfiguration contains a set of parameters to pass to the page 			
            _pageParameters = e.Parameter as AlbumItem;
            SolidColorBrush AlbumBrush = new SolidColorBrush(_pageParameters.Palette);
            AlbumDetailsHeader.Background = AlbumBrush;
            BitmapImage bmp = new BitmapImage(new Uri(_pageParameters.AlbumArtWork));
            AlbumArtWork.Source = bmp;
            AlbumSongsResources.Source = _pageParameters.Songs;
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
        {

        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
        }
    }
}
