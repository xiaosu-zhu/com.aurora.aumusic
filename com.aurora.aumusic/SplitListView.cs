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
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace com.aurora.aumusic
{
    public class SplitListView : ObservableCollection<Splitlist>
    {
        public SplitListView()
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            Add(new Splitlist(loader.GetString("AlbumFlowText"), new Uri("ms-appx:///Assets/ButtonIcon/albumflowbutton.png")));
            Add(new Splitlist(loader.GetString("ArtistText"), "\xE77B"));
            Add(new Splitlist(loader.GetString("SongsPageText"), "\xE8D6"));
            Add(new Splitlist(loader.GetString("LikeListText"), "\xE8A4"));
        }
    }

    public class Splitlist : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public string Title { get; set; }
        public string IconSymbol { get; set; }
        public Uri Bitmap { get; set; }
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
        public Splitlist(string title, Uri bitmap)
        {
            this.Title = title;
            this.Bitmap = bitmap;
            this.visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
