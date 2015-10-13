using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using KKBOX.Utility;

namespace com.aurora.aumusic
{
    public class BitmapHelper
    {

        public async Task<Color[]> New(Uri urisource)
        {
            Uri a = new Uri("ms-appx:///Assets/unknown.png");
            WriteableBitmap buffer = await BitmapFactory.New(1, 1).FromContent(a);
            Palette p;
            try
            {
                p = await Palette.Generate(buffer);
            }
            catch (Exception)
            {

                throw;
            }

            Color[] c = new Color[2];
            c[0] = p.getLightVibrantColor(Color.FromArgb((byte)204, (byte)240, (byte)240, (byte)240));
            c[1] = p.getDarkMutedColor(Color.FromArgb((byte)204, (byte)26, (byte)28, (byte)55));
            return c;
        }
    }
}
