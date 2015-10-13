using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;

namespace KKBOX.Utility
{
    public class Swatch
    {
        readonly int mRed, mGreen, mBlue;
        readonly int mPopulation;
        private Color mRgb;

        private float[] mHsl;

        public Swatch(Color rgbColor, int population)
        {
            mRed = rgbColor.R;
            mGreen = rgbColor.G;
            mBlue = rgbColor.B;
            mRgb = rgbColor;
            mPopulation = population;
        }

        public Swatch(int red, int green, int blue, int population)
        {
            mRed = red;
            mGreen = green;
            mBlue = blue;
            mRgb = Color.FromArgb(0xFF, (Byte)red, (Byte)green, (Byte)blue);
            mPopulation = population;
        }

        /**
         * Return this swatch's HSL values.
         *     hsv[0] is Hue [0 .. 360)
         *     hsv[1] is Saturation [0...1]
         *     hsv[2] is Lightness [0...1]
         */
        public float[] getHsl()
        {
            if (mHsl == null)
            {
                // Lazily generate HSL values from RGB
                mHsl = new float[3];
                ColorUtils.RGBtoHSL(mRed, mGreen, mBlue, mHsl);
            }
            return mHsl;
        }

        public Color GetRgb()
        {
            return mRgb;
        }

        /**
         * @return the number of pixels represented by this swatch
         */
        public int getPopulation()
        {
            return mPopulation;
        }
    }
}
