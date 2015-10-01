using System;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace com.aurora.aumusic
{
    class FolderItem
    {
        public String FolderPath;
        public BitmapImage FolderImg;
        public FolderItem(StorageFolder folder)
        {
            // FolderIndexToken.Add("SelectedFolder" + i);
            FolderPath = folder.Path;
            FolderImg = generateSelectedFolderRoot(folder);

        }

        public FolderItem()
        {
            // FolderIndexToken.Add("SelectedFolder" + i);
        }

        public void AddPath(StorageFolder folder)
        {
            FolderPath = folder.Path;
        }

        private BitmapImage generateSelectedFolderRoot(StorageFolder folder)
        {
            Char s = (folder.Path)[0];
            switch(s)
            {
                case 'C': return new BitmapImage(new Uri("ms-appx:///Assets/C.png"));
                case 'D': return new BitmapImage(new Uri("ms-appx:///Assets/D.png"));
                case 'E': return new BitmapImage(new Uri("ms-appx:///Assets/E.png"));
                case 'F': return new BitmapImage(new Uri("ms-appx:///Assets/F.png"));
                case 'G': return new BitmapImage(new Uri("ms-appx:///Assets/G.png"));
                case 'H': return new BitmapImage(new Uri("ms-appx:///Assets/H.png"));
                case 'I': return new BitmapImage(new Uri("ms-appx:///Assets/I.png"));
                case 'J': return new BitmapImage(new Uri("ms-appx:///Assets/J.png"));
                case 'K': return new BitmapImage(new Uri("ms-appx:///Assets/K.png"));
                case 'L': return new BitmapImage(new Uri("ms-appx:///Assets/L.png"));
                default: return new BitmapImage(new Uri("ms-appx:///Assets/unknown.png"));
            }
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
