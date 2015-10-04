using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace com.aurora.aumusic
{
    public sealed partial class SongsPage : Page
    {
        SongsEnum Songs = new SongsEnum();
        public SongsPage()
        {
            this.InitializeComponent();
        }

        private  void Progress_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //await Songs.GetSongsEnum(0);
        }

        
    }
}
