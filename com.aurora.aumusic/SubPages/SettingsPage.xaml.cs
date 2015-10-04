using System;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace com.aurora.aumusic
{
    public sealed partial class SettingsPage : Page
    {
        FolderPathObservation folderPaths = new FolderPathObservation();
        public SettingsPage()
        {

            this.InitializeComponent();
            MusicFolderPathReosurces.Source = folderPaths.GetFolders();
            //folderPaths.RestorePathsfromSettings();
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
                    //folderPaths.GetFolders();
                    folderPaths.SaveFoldertoSettings();
                }


            }
            else
            {

            }

        }
    }
}
