using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace com.aurora.aumusic
{
    class FolderPathObservation
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        ObservableCollection<FolderItem> Folders = new ObservableCollection<FolderItem>();
        public List<String> PathTokens = new List<string>();

        public async void RestorePathsfromSettings()
        {
            ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["FolderSettings"];
            if (composite != null)
            {
                String TempPath;
                int count = (int)composite["FolderCount"];
                Folders.Clear();
                PathTokens.Clear();
                for (int i = 0; i < count; i++)
                {
                    TempPath = (String)composite["FolderSettings" + i];
                    StorageFolder TempFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(TempPath);
                    if (TempFolder != null)
                    {
                        Folders.Add(GetNewFolder(TempFolder));
                        PathTokens.Add(TempPath);
                    }
                    else
                    {
                        break;
                    }
                }

            }

        }

        public int i = 0;


        public ObservableCollection<FolderItem> GetFolders()
        {
            return Folders;
        }

        public Boolean SaveFoldertoStorage(StorageFolder Folder)
        {
            FolderItem folder = GetNewFolder(Folder);
            foreach (var item in Folders)
            {
                if (item.FolderPath == folder.FolderPath)
                {
                    return false;
                }
            }
            PathTokens.Add(StorageApplicationPermissions.FutureAccessList.Add(Folder, Folder.Name));
            Folders.Add(folder);
            return true;

        }

        public void SaveFoldertoSettings()
        {
            ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();
            i = 0;
            foreach (var item in PathTokens)
            {
                composite["FolderSettings" + i] = item;
                i++;
            }
            composite["FolderCount"] = i;
            localSettings.Values["FolderSettings"] = composite;
        }

        private FolderItem GetNewFolder(StorageFolder folder)
        {
            return new FolderItem(folder);
        }
    }
}
