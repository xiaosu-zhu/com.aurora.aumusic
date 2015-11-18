using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace com.aurora.aumusic
{
    public sealed partial class CachePage: Page
    {
        public CachePage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.CacheImage.Source = e.Parameter as RenderTargetBitmap;
        }

        private void RootGird_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            CacheImage.Height = RootGird.ActualHeight;
        }
    }
}
