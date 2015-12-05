using System.Collections.Generic;
using com.aurora.aumusic.shared;
using com.aurora.aumusic.shared.Songs;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Navigation;
using com.aurora.aumusic.shared.MessageService;

namespace com.aurora.aumusic
{
    public sealed partial class ListPage : Page
    {
        CurrentTheme Theme = ((Window.Current.Content as Frame).Content as MainPage).Theme;
        public static List<Song> AllSongs;
        ObservableCollection<Song> AllSongsViewModel = new ObservableCollection<Song>();

        public ListPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            App.ResetTitleBar();
            SongListSource.Source = AllSongsViewModel;
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            var song = new SongModel(((sender as Button).DataContext as Song));
            var list = new List<SongModel>();
            foreach (var item in AllSongs)
            {
                list.Add(new SongModel(item));
            }
            MessageService.SendMessageToBackground(new ForePlaybackChangedMessage((PlaybackState.Playing), list, song));
        }

        private void LadingRing_Loaded(object sender, RoutedEventArgs e)
        {
            if (AllSongs == null || AllSongs.Count == 0)
                return;
            AllSongsViewModel.Clear();
            foreach (var item in AllSongs)
            {
                if(item.Loved == true)
                {
                    AllSongsViewModel.Add(item);
                }
            }
            LadingRing.IsActive = false;
            LadingRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
    }
}
