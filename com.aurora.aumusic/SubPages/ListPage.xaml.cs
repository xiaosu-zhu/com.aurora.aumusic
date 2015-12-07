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
