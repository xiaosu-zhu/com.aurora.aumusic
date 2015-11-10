using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace com.aurora.aumusic.shared.FolderSettings
{
    public class FolderItem
    {
        public StorageFolder Folder;
        public char Key;
        public FolderItem(StorageFolder folder)
        {
            this.Folder = folder;
            this.Key = folder.Path[0];
        }
    }
}
