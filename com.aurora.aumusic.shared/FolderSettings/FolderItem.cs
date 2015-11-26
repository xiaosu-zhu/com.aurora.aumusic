using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace com.aurora.aumusic.shared.FolderSettings
{
    public class FolderItem : INotifyPropertyChanged
    {
        public StorageFolder Folder;
        public Visibility _visibility = Visibility.Collapsed;
        public Visibility visibility
        {
            get
            {
                return _visibility;
            }
            set
            {
                _visibility = value;
                this.OnPropertyChanged();
            }
        }
        public char Key;
        public int i;

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public FolderItem(StorageFolder folder)
        {
            this.Folder = folder;
            this.Key = folder.Path[0];
        }

        public FolderItem(StorageFolder folder, int i) : this(folder)
        {
            this.i = i;
        }
    }
}
