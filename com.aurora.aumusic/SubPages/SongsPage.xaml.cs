using com.aurora.aumusic.shared.Helpers;
using com.aurora.aumusic.shared.MessageService;
using com.aurora.aumusic.shared.Songs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace com.aurora.aumusic
{
    public sealed partial class SongsPage : Page
    {
        public static List<Song> AllSongs;
        ObservableCollection<AlphaKeyGroup<Song>> AllSongsViewModel = new ObservableCollection<AlphaKeyGroup<Song>>();
        List<SongModel> SongModels;

        public SongsPage()
        {
            this.InitializeComponent();
            SongListSource.Source = AllSongsViewModel;
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            ThreadPool.RunAsync((work) =>
            {
                if (AllSongs != null)
                {
                    SongModels = new List<SongModel>();
                    foreach (var item in AllSongs)
                    {
                        SongModels.Add(new SongModel(item));
                    }
                }

            });
        }

        private void LadingRing_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (AllSongs != null)
            {
                AllSongsViewModel.Clear();
                var grouplist = (AlphaKeyGroup<Song>.CreateGroups(AllSongs, song => { return song.Title; }, true));
                foreach (var item in grouplist)
                {
                    if (item.Count == 0)
                        continue;
                    AllSongsViewModel.Add(item);
                }
            }
            LadingRing.IsActive = false;
            LadingRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        private void PlayButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (SongModels != null)
                MessageService.SendMessageToBackground(new ForePlaybackChangedMessage(PlaybackState.Playing, SongModels, new SongModel((sender as Button).DataContext as Song)));
        }
    }
}
