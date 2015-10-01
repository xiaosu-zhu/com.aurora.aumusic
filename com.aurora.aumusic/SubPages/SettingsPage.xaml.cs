using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                folderPaths.SaveFoldertoStorage(folder);
                folderPaths.GetFolders();
                //TODO: 更改绑定方式(参考ListView Template)

            }
            else
            {

            }

        }
    }
}
