using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;

namespace KKBOX.Utility
{
    public class ColorUtils
    {
        private ColorUtils() { }

        /**
         * @return luma value according to to XYZ color space in the range 0.0 - 1.0
         */
        static float calculateXyzLuma(int rgbColor)
        {
            var color = Color.FromArgb((Byte)(rgbColor & 0xff000000 >> 32), (Byte)(rgbColor & 0x000000ff), (Byte)((rgbColor & 0x0000ff00) >> 8), (Byte)((rgbColor & 0x00ff0000) >> 16));

            return (0.2126f * color.R +
                    0.7152f * color.G +
                    0.0722f * color.B) / 255f;
        }

        static float calculateContrast(int color1, int color2)
        {
            return Math.Abs(ColorUtils.calculateXyzLuma(color1) - ColorUtils.calculateXyzLuma(color2));
        }

        public static void RGBtoHSL(int r, int g, int b, float[] hsl)
        {
            float rf = r / 255f;
            float gf = g / 255f;
            float bf = b / 255f;

            float max = Math.Max(rf, Math.Max(gf, bf));
            float min = Math.Min(rf, Math.Min(gf, bf));
            float deltaMaxMin = max - min;

            float h, s;
            float l = (max + min) / 2f;

            if (max == min)
            {
                // Monochromatic
                h = s = 0f;
            }
            else
            {
                if (max == rf)
                {
                    h = ((gf - bf) / deltaMaxMin) % 6f;
                }
                else if (max == gf)
                {
                    h = ((bf - rf) / deltaMaxMin) + 2f;
                }
                else
                {
                    h = ((rf - gf) / deltaMaxMin) + 4f;
                }

                s = deltaMaxMin / (1f - Math.Abs(2f * l - 1f));
            }

            hsl[0] = (h * 60f) % 360f;
            hsl[1] = s;
            hsl[2] = l;
        }

        public static Color HSLtoRGB(float[] hsl)
        {
            float h = hsl[0];
            float s = hsl[1];
            float l = hsl[2];

            float c = (1f - Math.Abs(2 * l - 1f)) * s;
            float m = l - 0.5f * c;
            float x = c * (1f - Math.Abs((h / 60f % 2f) - 1f));

            int hueSegment = (int)h / 60;

            int r = 0, g = 0, b = 0;

            switch (hueSegment)
            {
                case 0:
                    r = (Int32)Math.Round(255 * (c + m));
                    g = (Int32)Math.Round(255 * (x + m));
                    b = (Int32)Math.Round(255 * m);
                    break;
                case 1:
                    r = (Int32)Math.Round(255 * (x + m));
                    g = (Int32)Math.Round(255 * (c + m));
                    b = (Int32)Math.Round(255 * m);
                    break;
                case 2:
                    r = (Int32)Math.Round(255 * m);
                    g = (Int32)Math.Round(255 * (c + m));
                    b = (Int32)Math.Round(255 * (x + m));
                    break;
                case 3:
                    r = (Int32)Math.Round(255 * m);
                    g = (Int32)Math.Round(255 * (x + m));
                    b = (Int32)Math.Round(255 * (c + m));
                    break;
                case 4:
                    r = (Int32)Math.Round(255 * (x + m));
                    g = (Int32)Math.Round(255 * m);
                    b = (Int32)Math.Round(255 * (c + m));
                    break;
                case 5:
                case 6:
                    r = (Int32)Math.Round(255 * (c + m));
                    g = (Int32)Math.Round(255 * m);
                    b = (Int32)Math.Round(255 * (x + m));
                    break;
            }

            r = Math.Max(0, Math.Min(255, r));
            g = Math.Max(0, Math.Min(255, g));
            b = Math.Max(0, Math.Min(255, b));

            return Color.FromArgb((byte)255, (byte)r, (byte)g, (byte)b);
        }

        public static Color DiffColor(Color originalColor)
        {
            Int32 r = 0;
            Int32 g = 0;
            Int32 b = 0;

            r = (byte)0xFF ^ originalColor.R;
            g = (byte)0xFF ^ originalColor.G;
            b = (byte)0xFF ^ originalColor.B;

            return Color.FromArgb(0xFF, (byte)r, (byte)g, (byte)b);
        }

        public static Color SimilarColor(Color originalColor)
        {
            float[] mTempHsl = new float[3];
            RGBtoHSL(originalColor.R, originalColor.G, originalColor.B, mTempHsl);

            Int32 index = 0;
            float baseMovement = 0.05f;
            float baseParameter = 0.35f;

            do
            {
                float currentMovement = (baseParameter - (index * baseMovement));
                float newLumaUpper = mTempHsl[2] + currentMovement;
                float newLumaLower = mTempHsl[2] - currentMovement;

                if (newLumaUpper <= 1.0f)
                {
                    mTempHsl[2] = newLumaUpper;
                    return HSLtoRGB(mTempHsl);
                }
                else if (newLumaLower >= 0f)
                {
                    mTempHsl[2] = newLumaLower;
                    return HSLtoRGB(mTempHsl);
                }

                index++;
            } while (index <= 7);

            mTempHsl[2] = 0.3f;
            return HSLtoRGB(mTempHsl);
        }
    }
}
