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
using com.aurora.aumusic.shared.FolderSettings;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace com.aurora.aumusic
{
    public sealed partial class StartPage:Page
    {
        FolderPathObservation folderPaths = new FolderPathObservation();
        public StartPage()
        {
            this.InitializeComponent();
            MusicFolderPathReosurces.Source = folderPaths.GetFolders();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var view = ApplicationView.GetForCurrentView();
            ApplicationViewTitleBar titleBar = view.TitleBar;
            if (titleBar != null)
            {
                titleBar.BackgroundColor = Color.FromArgb(255, 0, 116, 206);
                titleBar.ButtonBackgroundColor = Color.FromArgb(255, 0, 116, 206);
            }
        }

        private async void StackPanel_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var p = await folderPaths.RestorePathsfromSettings();
            if (p != null)
            {
                foreach (var item in p)
                {
                    folderPaths.GetFolders().Add(item);
                }
            }

        }

        private async void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add(".mp3");
            folderPicker.FileTypeFilter.Add(".m4a");
            folderPicker.FileTypeFilter.Add(".flac");
            folderPicker.FileTypeFilter.Add(".aac");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                if (folderPaths.SaveFoldertoStorage(folder))
                {
                    await Task.Run(() =>
                    {
                        folderPaths.SaveFoldertoSettings();
                    });
                }

            }
            else
            {

            }

        }

        private void FinishStartButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (!localSettings.Values.ContainsKey("FolderSettings"))
            {
                return;
            }
            else
            {
                (Window.Current.Content as Frame).Navigate(typeof(MainPage));
            }
        }
    }
}
