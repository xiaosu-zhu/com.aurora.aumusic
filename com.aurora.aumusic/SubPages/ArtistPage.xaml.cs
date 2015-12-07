//Copyright(C) 2015 Aurora Studio

//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
//to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
//WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.



/// <summary>
/// Usings
/// </summary>
using System;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using com.aurora.aumusic.shared.Songs;
using com.aurora.aumusic.shared.MessageService;
using com.aurora.aumusic.shared.Albums;
using System.Collections.ObjectModel;
using com.aurora.aumusic.shared.Helpers;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using com.aurora.aumusic.shared;

namespace com.aurora.aumusic
{
    public sealed partial class ArtistPage : Page
    {
        public static List<AlbumItem> AllSongs;
        ObservableCollection<ArtistsKeyGroup<AlbumItem>> ArtistsGroupViewModel = new ObservableCollection<ArtistsKeyGroup<AlbumItem>>();
        CurrentTheme Theme = ((Window.Current.Content as Frame).Content as MainPage).Theme;
        public ArtistPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            App.ResetTitleBar();
            ArtistsSource.Source = ArtistsGroupViewModel;
            ArtistDetailsSource.Source = null;
            SystemNavigationManager.GetForCurrentView().BackRequested += Zoom_BackRequested;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= this.Zoom_BackRequested;
            SemanticZoom.ViewChangeStarted -= SemanticZoom_ViewChangeStarted;
        }



        private async void LoadingRing_Loaded(object sender, RoutedEventArgs e)
        {
            if (AllSongs == null || AllSongs.Count == 0)
                return;
            await ThreadPool.RunAsync((work) =>
            {
                    var query = ArtistsKeyGroup<AlbumItem>.CreateGroups(AllSongs, album => album.AlbumArtists, true);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                this.Dispatcher.RunAsync(CoreDispatcherPriority.High, new DispatchedHandler(() =>
                     {
                         ArtistsGroupViewModel.Clear();
                         foreach (var g in query)
                         {
                             List<string> artworks = new List<string>();
                             foreach (var item in g)
                             {
                                 artworks.Add(item.AlbumArtWork);
                             }
                             g.SetArtworks(artworks.ToArray());
                             ArtistsGroupViewModel.Add(g);
                         }
                     }));
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            });
            LoadingRing.IsActive = false;
            LoadingRing.Visibility = Visibility.Collapsed;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var album = ((sender as Button).DataContext as Song).Parent as AlbumItem;
            MessageService.SendMessageToBackground(new ForePlaybackChangedMessage(PlaybackState.Playing, album.ToSongModelList(), new SongModel((sender as Button).DataContext as Song)));
        }

        private void SemanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (SemanticZoom.IsZoomedInViewActive)
            {
                SystemNavigationManager.GetForCurrentView().BackRequested += Zoom_BackRequested;
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                var list = AlbumSongsGroup.CreateGroup(e.SourceItem.Item as ArtistsKeyGroup<AlbumItem>, true);
                if (list == null)
                    SemanticZoom.IsZoomedInViewActive = false;
                ArtistDetailsSource.Source = list;
                ArtistDetailedView.ItemsSource = ArtistDetailsSource.View;
                SemanticZoom.CanChangeViews = false;
                var ArtistArtworkGroups = ArtistDetailedView.FindChildControl<RelativePanel>("ArtistArtworkGroups") as RelativePanel;
                var ArtistArtworkGroup0 = ArtistDetailedView.FindChildControl<RelativePanel>("ArtistArtworkGroup0") as RelativePanel;
                var ArtistArtworkGroup1 = ArtistDetailedView.FindChildControl<RelativePanel>("ArtistArtworkGroup1") as RelativePanel;
                var ArtistArtworkGroup2 = ArtistDetailedView.FindChildControl<RelativePanel>("ArtistArtworkGroup2") as RelativePanel;

                var ArtistName = ArtistDetailedView.FindChildControl<TextBlock>("ArtistName") as TextBlock;
                var ArtistDetails = ArtistDetailedView.FindChildControl<TextBlock>("ArtistDetails") as TextBlock;

                var artistsconverter = new ArtistsConverter();
                var artistdetailsconverter = new ArtistDetailsConverter();
                var artistdetails = artistdetailsconverter.Convert(list, null, null, null);
                var artists = artistsconverter.Convert(list[0].AlbumArtists, null, true, null);
                ArtistDetails.Text = (string)artistdetails;
                ArtistName.Text = (string)artists;
                var imagelist = ArtistArtworkGroups.GetImages();
                foreach (var image in imagelist)
                {
                    image.Source = null;
                }
                

                if (list.Count < 5)
                {
                    ArtistArtworkGroup0.Height = 400;
                    ArtistArtworkGroup1.Height = 400;
                    ArtistArtworkGroup2.Height = 400;
                }
                else if (list.Count < 9)
                {
                    ArtistArtworkGroup0.Height = 320;
                    ArtistArtworkGroup1.Height = 320;
                    ArtistArtworkGroup2.Height = 320;
                }
                else
                {
                    ArtistArtworkGroup0.Height = 240;
                    ArtistArtworkGroup1.Height = 240;
                    ArtistArtworkGroup2.Height = 240;
                }
                int i = 0;
                var placeholder = new BitmapImage();
                foreach (var image in imagelist)
                {
                    if (list.Count < i + 1)
                        image.Source = placeholder;
                    else
                        image.Source = new BitmapImage(new Uri(list[i].AlbumArtWork));
                    i++;
                }
            }
        }

        private void Zoom_BackRequested(object sender, BackRequestedEventArgs e)
        {
            SemanticZoom.CanChangeViews = true;
            SemanticZoom.IsZoomedInViewActive = false;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            SystemNavigationManager.GetForCurrentView().BackRequested -= Zoom_BackRequested;
            e.Handled = true;
        }

        private void GroupPlayButton_Click(object sender, RoutedEventArgs e)
        {
            var list = new List<SongModel>();
            foreach (var item in ((sender as Button).DataContext as ArtistsKeyGroup<AlbumItem>))
            {
                list.AddRange(item.ToSongModelList());
            }
            MessageService.SendMessageToBackground(new ForePlaybackChangedMessage(PlaybackState.Playing, list, list[0]));
        }

        private void RelativePanel_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            //TODO:
        }

        private void ArtistPlayButton_Click(object sender, RoutedEventArgs e)
        {
            var list = new List<SongModel>();
            foreach (var item in (ArtistDetailsSource.Source as List<AlbumSongsGroup>))
            {
                var album = item[0].Parent;
                list.AddRange(album.ToSongModelList());
            }
            MessageService.SendMessageToBackground(new ForePlaybackChangedMessage(PlaybackState.Playing, list, list[0]));
        }
    }
}
