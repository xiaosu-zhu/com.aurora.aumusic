using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace com.aurora.aumusic
{
    class FolderItem
    {
        public String FolderPath;
        private int i = 0;
        public FolderItem(StorageFolder folder)
        {
            // FolderIndexToken.Add("SelectedFolder" + i);
            FolderPath = folder.Path;

        }

        public FolderItem()
        {
            // FolderIndexToken.Add("SelectedFolder" + i);
        }

        public void AddPath(StorageFolder folder)
        {
            FolderPath = folder.Path;
        }

        public String generateSelectedFolderPath(StorageFolder folder)
        {
            return folder.Path;
        }




        //public void SaveFoldertoStorage(StorageFolder folder)
        //{
        //    for (;;)
        //    {
        //        if (Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(FolderIndexToken[i]))
        //        {
        //            i++;
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    i++;
        //    FolderIndexToken.Add("SelectedFolder" + i);
        //    Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder, FolderIndexToken[i]);
        //}

    }
}
