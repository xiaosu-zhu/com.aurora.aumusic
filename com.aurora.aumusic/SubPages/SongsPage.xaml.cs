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
using com.aurora.aumusic.shared.Helpers;
using com.aurora.aumusic.shared.Songs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System;
using Windows.System.Threading;
using com.aurora.aumusic.shared.MessageService;
using com.aurora.aumusic.shared;
using Windows.UI.Xaml;

namespace com.aurora.aumusic
{
    public sealed partial class SongsPage : Page
    {
        public static List<Song> AllSongs;
        ObservableCollection<AlphaKeyGroup<Song>> AllSongsViewModel = new ObservableCollection<AlphaKeyGroup<Song>>();
        List<SongModel> SongModels;
        CurrentTheme Theme = ((Window.Current.Content as Frame).Content as MainPage).Theme;

        public SongsPage()
        {
            this.InitializeComponent();
            App.ResetTitleBar();
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

        private async void LadingRing_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (AllSongs == null || AllSongs.Count == 0)
                return;
            await ThreadPool.RunAsync((work) =>
             {
                     var grouplist = (AlphaKeyGroup<Song>.CreateGroups(AllSongs, song => { return song.Title; }, true));
                     this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, new Windows.UI.Core.DispatchedHandler(() =>
                      {
                          AllSongsViewModel.Clear();
                          foreach (var item in grouplist)
                          {
                              if (item.Count == 0)
                                  continue;
                              AllSongsViewModel.Add(item);
                          }
                      }));
             });
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
