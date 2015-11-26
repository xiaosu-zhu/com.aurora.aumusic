﻿using com.aurora.aumusic.shared.FolderSettings;
using System;
using System.Collections.Generic;
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

        private void MusicSettingsSearchingList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in (folderPaths.GetFolders()))
            {
                foreach (FolderItem f in item.PathList)
                {
                    f.visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }

            var folder = MusicSettingsSearchingList.SelectedItem as FolderItem;
            folder.visibility = Windows.UI.Xaml.Visibility.Visible;
        }
    }

}
