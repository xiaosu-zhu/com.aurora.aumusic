using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace com.aurora.aumusic
{
    public sealed partial class AlbumDetails : Page
    {
        PlaybackPack _pageParameters;
        private static double _verticalPosition = 0.0;
        private static double _delta;
        private static double HeaderHeight;
        ScrollViewer s;

        public int MouseWheelCount { get; private set; }
        public double MaxScrollHeight { get; private set; }

        public AlbumDetails()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
        }

        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = this.Parent as Frame;
            if (rootFrame == null)
                return;

            // Navigate back if possible, and if the event has not 
            // already been handled .
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            //We are going to cast the property Parameter of NavigationEventArgs object
            //into PageWithParametersConfiguration.
            //PageWithParametersConfiguration contains a set of parameters to pass to the page 			
            _pageParameters = e.Parameter as PlaybackPack;
            SolidColorBrush AlbumBrush = new SolidColorBrush(_pageParameters.Album.Palette);
            AlbumDetailsHeader.Background = AlbumBrush;
            var view = ApplicationView.GetForCurrentView();
            ApplicationViewTitleBar titleBar = view.TitleBar;
            titleBar.BackgroundColor = _pageParameters.Album.Palette;
            titleBar.ButtonBackgroundColor = _pageParameters.Album.Palette;
            BitmapImage bmp = new BitmapImage(new Uri(_pageParameters.Album.AlbumArtWork));
            AlbumArtWork.Source = bmp;
            AlbumSongsResources.Source = _pageParameters.Album.Songs;
            HeaderHeight = AlbumDetailsHeader.Height;

        }

        private void ScrollViewer_PointerWheelChanged(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            PointerPoint p = e.GetCurrentPoint(s);
            _verticalPosition -= p.Properties.MouseWheelDelta * _delta;
            if (_verticalPosition > MaxScrollHeight)
            {
                _verticalPosition = MaxScrollHeight;
            }
            if (_verticalPosition < 0)
            {
                _verticalPosition = 0;
            }
            AlbumDetailsHeader.Height = HeaderHeight - 2 * _verticalPosition >= 0 ? HeaderHeight - 2 * _verticalPosition : 0;
            s.ChangeView(0, _verticalPosition, 1);
        }

        private void ScrollViewer_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            s = sender as ScrollViewer;
            MaxScrollHeight = s.ScrollableHeight;
            _delta = MaxScrollHeight / (_pageParameters.Album.Songs.Count * 120);
            if (MaxScrollHeight == 0)
            {
                s.PointerWheelChanged -= ScrollViewer_PointerWheelChanged;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Song s = ((Button)sender).DataContext as Song;
            List<Song> tempSongList = new List<Song>();
            int index = _pageParameters.Album.Songs.IndexOf(s);
            tempSongList = _pageParameters.Album.Songs.GetRange(index, _pageParameters.Album.Songs.Count - index);
            await _pageParameters.PlaybackControl.Play(tempSongList, _pageParameters.Media);
        }
    }
}
