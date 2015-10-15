﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace com.aurora.aumusic
{
    public sealed partial class AlbumDetails : Page
    {
        AlbumItem _pageParameters;
        private static double _verticalPosition = 0.0;
        private static double _delta = 80;
        private static double HeaderHeight;

        public int MouseWheelCount { get; private set; }

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
            HeaderHeight = AlbumDetailsHeader.Height;
        }

        private void ScrollViewer_PointerWheelChanged(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            ScrollViewer s = sender as ScrollViewer;
            if (s.ScrollableHeight > 0)
            {
                PointerPoint p = e.GetCurrentPoint(s);
                ItemsPresenter t = s.Content as ItemsPresenter;
                _verticalPosition -= (p.Properties.MouseWheelDelta / 120) * _delta;
                if (_verticalPosition < 0)
                {
                    _verticalPosition = 0;
                }
                if (_verticalPosition > s.ScrollableHeight)
                {
                    _verticalPosition = s.ScrollableHeight;
                }
                AlbumDetailsHeader.Height = HeaderHeight - _verticalPosition >= 0 ? HeaderHeight - _verticalPosition : 0;
                s.ChangeView(0, _verticalPosition, 1);
            }
        }


    }
}