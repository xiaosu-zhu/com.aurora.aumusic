using com.aurora.aumusic.shared.FolderSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
