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
        ObservableCollection<FolderItem> Folders = new ObservableCollection<FolderItem>();
        public List<FolderItem> FolderList = new List<FolderItem>();
        public int i = 0;


        public ObservableCollection<FolderItem> GetFolders()
        {
            Folders.Clear();
            for (i = 0; i<FolderList.Count; i++)
            {
                Folders.Add(FolderList[i]);
            }
            return Folders;
        }

        public void SaveFoldertoStorage(StorageFolder folder)
        {
            FolderList.Add(GetNewFolder(folder));
        }

        private FolderItem GetNewFolder(StorageFolder folder)
        {
            return new FolderItem(folder);
        }
    }
}
