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
                var query = from item in AllSongs
                            group item by item.AlbumArtists into g
                            orderby g.Key[0]
                            select new { GroupName = g.Key, Items = g };
                foreach (var g in query)
                {
                    ArtistsKeyGroup<AlbumItem> albums = new ArtistsKeyGroup<AlbumItem>(g.GroupName);
                    albums.AddRange(g.Items);
                    ArtistsGroupViewModel.Add(albums);
                }
            }
            LoadingRing.IsActive = false;
            LoadingRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
    }
}
