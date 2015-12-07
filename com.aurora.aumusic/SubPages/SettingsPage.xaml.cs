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
using com.aurora.aumusic.shared;
using com.aurora.aumusic.shared.FolderSettings;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace com.aurora.aumusic
{
    public sealed partial class SettingsPage : Page
    {
        FolderPathObservation folderPaths = new FolderPathObservation();
        private Button deletebutton;
        CurrentTheme Theme = ((Window.Current.Content as Frame).Content as MainPage).Theme;

        public SettingsPage()
        {
            this.InitializeComponent();
            MusicFolderPathReosurces.Source = folderPaths.GetFolders();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            App.ResetTitleBar();
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
            if (folder != null)
                folder.visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private async void DeleteConfirmButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            deletebutton.Flyout.Hide();
            folderPaths.DeleteFolder((deletebutton.DataContext as FolderItem));
            var p = await folderPaths.RestorePathsfromSettings();
            if (p != null)
            {
                foreach (var item in p)
                {
                    folderPaths.GetFolders().Add(item);
                }
            }
        }

        private void DeleteCancelButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            deletebutton.Flyout.Hide();
        }

        private void DeleteButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            deletebutton = (sender as Button);
        }

        private void ThemeSettingsBox_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var str = (string)ApplicationSettingsHelper.ReadSettingsValue("ThemeSettings");
            if (str == null)
            {
                ThemeSettingsBox.SelectedIndex = 0;
            }
            else if (str == "auto")
            {
                ThemeSettingsBox.SelectedIndex = 0;
            }
            else if (str == "Light")
            {
                ThemeSettingsBox.SelectedIndex = 1;
            }
            else if (str == "Dark")
            {
                ThemeSettingsBox.SelectedIndex = 2;
            }
            ThemeSettingsBox.SelectionChanged += ThemeSettingsBox_SelectionChanged;
        }

        private void ThemeSettingsBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeSettingsBox.SelectedIndex == 0)
            {
                ApplicationSettingsHelper.SaveSettingsValue("ThemeSettings","auto");
                ApplicationSettingsHelper.SetAutoTheme(Theme);
            }
            else if(ThemeSettingsBox.SelectedIndex ==1)
            {
                ApplicationSettingsHelper.SaveSettingsValue("ThemeSettings", "Light");
                Theme.Theme = ElementTheme.Light;
            }
            else if (ThemeSettingsBox.SelectedIndex == 2)
            {
                ApplicationSettingsHelper.SaveSettingsValue("ThemeSettings", "Dark");
                Theme.Theme = ElementTheme.Dark;
            }
        }

        private void ToggleSwitch_Loaded(object sender, RoutedEventArgs e)
        {
            var str = (string)ApplicationSettingsHelper.ReadSettingsValue("AutoLyric");
            if (str == null)
            {
                AutoLyricSwitch.IsOn = true;
            }
            else if(str == "true")
            {
                AutoLyricSwitch.IsOn = true;
            }
            else if (str == "false")
            {
                AutoLyricSwitch.IsOn = false;
            }
            AutoLyricSwitch.Toggled += AutoLyricSwitch_Toggled;
        }

        private void AutoLyricSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            switch (AutoLyricSwitch.IsOn)
            {
                case true:ApplicationSettingsHelper.SaveSettingsValue("AutoLyric", "true"); break;
                case false:ApplicationSettingsHelper.SaveSettingsValue("AutoLyric", "false"); break;
                default:
                    break;
            }
        }
    }

}
