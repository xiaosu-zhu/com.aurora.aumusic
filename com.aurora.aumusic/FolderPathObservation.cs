using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace com.aurora.aumusic
{
    class FolderPathObservation
    {
        Windows.Storage.ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        ObservableCollection<FolderItem> Folders = new ObservableCollection<FolderItem>();

        public async void RestorePathsfromSettings()
        {
            String TempPath;
            int count = (int)localSettings.Values["FolderCount"];
            Folders.Clear();
            for (int i = 0; i < count; i++)
            {
                TempPath = (String)localSettings.Values["FolderSettings" + i];
                StorageFolder TempFolder = await StorageFolder.GetFolderFromPathAsync(TempPath);
                Folders.Add(GetNewFolder(TempFolder));
            }

        }

        public List<FolderItem> FolderList = new List<FolderItem>();
        public int i = 0;


        public ObservableCollection<FolderItem> GetFolders()
        {
            Folders.Clear();
            for (i = 0; i < FolderList.Count; i++)
            {
                Folders.Add(FolderList[i]);
            }
            return Folders;
        }

        public void SaveFoldertoStorage(StorageFolder folder)
        {
            FolderList.Add(GetNewFolder(folder));
        }

        public void SaveFoldertoSettings()
        {
            i = 0;
            foreach (var item in Folders)
            {
                localSettings.Values["FolderSettings" + i] = item.FolderPath;
                i++;
            }
            localSettings.Values["FolderCount"] = i;
        }

        private FolderItem GetNewFolder(StorageFolder folder)
        {
            return new FolderItem(folder);
        }
    }
}
