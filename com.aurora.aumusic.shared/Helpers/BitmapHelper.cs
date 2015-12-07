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
using System.IO;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace com.aurora.aumusic.shared
{
    public class BitmapHelper
    {
        private static readonly int CALCULATE_BITMAP_MIN_DIMENSION = 50;
        static Color[] pixels;

        private static async Task<Color> GetPixels(WriteableBitmap bitmap, Color[] pixels, Int32 width, Int32 height)
        {
            IRandomAccessStream bitmapStream = new InMemoryRandomAccessStream();
            await bitmap.ToStreamAsJpeg(bitmapStream);
            var bitmapDecoder = await BitmapDecoder.CreateAsync(bitmapStream);
            var pixelProvider = await bitmapDecoder.GetPixelDataAsync();
            Byte[] byteArray = pixelProvider.DetachPixelData();
            Int32 r = 0, g = 0, b = 0;
            int sum = pixels.Length;
            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {

                    r += byteArray[(i * width + j) * 4 + 2];
                    g += byteArray[(i * width + j) * 4 + 1];
                    b += byteArray[(i * width + j) * 4 + 0];
                }
            }
            return Color.FromArgb((byte)(255), (byte)(r / sum), (byte)(g / sum), (byte)(b / sum));
        }
        private static async Task<Color> fromBitmap(WriteableBitmap bitmap)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            pixels = new Color[width * height];
            return await GetPixels(bitmap, pixels, width, height);
        }
        private static WriteableBitmap scaleBitmapDown(WriteableBitmap bitmap)
        {
            int minDimension = Math.Min(bitmap.PixelWidth, bitmap.PixelHeight);

            if (minDimension <= CALCULATE_BITMAP_MIN_DIMENSION)
            {
                // If the bitmap is small enough already, just return it
                return bitmap;
            }

            float scaleRatio = CALCULATE_BITMAP_MIN_DIMENSION / (float)minDimension;

            WriteableBitmap resizedBitmap = bitmap.Resize((Int32)(bitmap.PixelWidth * scaleRatio), (Int32)(bitmap.PixelHeight * scaleRatio), WriteableBitmapExtensions.Interpolation.Bilinear);
            return resizedBitmap;
        }
        public static async Task<Color> New(Uri urisource)
        {
            WriteableBitmap buffer = await BitmapFactory.New(1, 1).FromContent(urisource);
            WriteableBitmap scaledbmp = scaleBitmapDown(buffer);
            return await fromBitmap(scaledbmp);
        }

        public static async Task<Color> New(Stream stream)
        {
            WriteableBitmap map = await BitmapFactory.New(1, 1).FromStream(stream);
            WriteableBitmap scaledbmp = scaleBitmapDown(map);
            return await fromBitmap(scaledbmp);
        }
    }
}
