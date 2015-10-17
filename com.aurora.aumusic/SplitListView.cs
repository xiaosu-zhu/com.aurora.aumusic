using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic
{
    public class SplitListView : ObservableCollection<Splitlist>
    {
        public SplitListView()
        {
            Add(new Splitlist("AlbumFlow", "Bookmarks"));
            Add(new Splitlist("Artists", "Contact"));
            Add(new Splitlist("Songs", "Rotate"));
            Add(new Splitlist("Song Lists", "Bookmarks"));
        }
    }

    public class Splitlist : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public string Title { get; set; }
        public string IconSymbol { get; set; }
        private Windows.UI.Xaml.Visibility _visibility;
        public Windows.UI.Xaml.Visibility visibility
        {
            get
            {
                return this._visibility; 
            }
            set
            {
                this._visibility = value;
                this.OnPropertyChanged();
            }
        }
        public Splitlist(string title, string iconsymbol)
        {
            this.Title = title;
            this.IconSymbol = iconsymbol;
            this.visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
