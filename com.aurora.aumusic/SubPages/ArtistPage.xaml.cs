using com.aurora.aumusic.shared.Albums;
using com.aurora.aumusic.shared.Helpers;
using com.aurora.aumusic.shared.Songs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace com.aurora.aumusic
{
    public sealed partial class ArtistPage : Page
    {
        public static List<AlbumItem> AllSongs;
        static List<SongModel> SongsModel;
        ObservableCollection<ArtistsKeyGroup<AlbumItem>> ArtistsGroupViewModel = new ObservableCollection<ArtistsKeyGroup<AlbumItem>>();

        public ArtistPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ArtistsSource.Source = ArtistsGroupViewModel;
        }

        private void LoadingRing_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ArtistsGroupViewModel.Clear();
            if (AllSongs != null)
            {
                var query = ArtistsKeyGroup<AlbumItem>.CreateGroups(AllSongs, album => album.AlbumArtists, true);
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
            }
            LoadingRing.IsActive = false;
            LoadingRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }
    }
}
