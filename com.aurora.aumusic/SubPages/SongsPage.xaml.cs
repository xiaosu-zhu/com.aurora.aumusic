using com.aurora.aumusic.shared.Helpers;
using com.aurora.aumusic.shared.Songs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace com.aurora.aumusic
{
    public sealed partial class SongsPage : Page
    {
        List<Song> AllSongs;
        ObservableCollection<AlphaKeyGroup<Song>> AllSongsViewModel = new ObservableCollection<AlphaKeyGroup<Song>>();

        public SongsPage()
        {
            this.InitializeComponent();
            SongListSource.Source = AllSongsViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null && e.Parameter is List<Song>)
                AllSongs = (List<Song>)e.Parameter;
        }

        private void LadingRing_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (AllSongs != null)
            {
                AllSongsViewModel.Clear();
                var grouplist = (AlphaKeyGroup<Song>.CreateGroups(AllSongs, song => { return song.Title; }, true));
                foreach (var item in grouplist)
                {
                    AllSongsViewModel.Add(item);
                }
            }
        }
    }
}
