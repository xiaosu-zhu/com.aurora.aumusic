using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace com.aurora.aumusic
{
    class FolderItem
    {
        public FolderItem()
        {
            FolderPath = " ";
        }

        private String folderPath;
        public String FolderPath
        {
            get
            {
                return folderPath;
            }
            
            set
            {
                folderPath = value;
            }
        }

        public void generateSelectedFolderPath(StorageFolder folder)
        {
            FolderPath = folder.Path;
        }


    }
}
