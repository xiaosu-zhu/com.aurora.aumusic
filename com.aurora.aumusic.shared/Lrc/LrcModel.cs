using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kfstorm.LrcParser;
using Windows.UI;
using Windows.UI.Xaml.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace com.aurora.aumusic.shared.Lrc
{
    public class LrcModel :INotifyPropertyChanged
    {
        public IOneLineLyric Lyric;
        private Brush _maincolor;

        public LrcModel(IOneLineLyric item)
        {
            this.Lyric = item;
        }

        public Brush MainColor
        {
            get
            {
                return _maincolor;
            }
            set
            {
                _maincolor = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
