using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI;

namespace KKBOX.Utility
{
    public class ColorHistogram
    {
        private readonly Color[] mColors;
        private readonly int[] mColorCounts;
        private readonly int mNumberColors;

        public ColorHistogram(Color[] pixels)
        {
            // Sort the pixels to enable counting below
            Array.Sort(pixels, new ColorComparer());

            // Count number of distinct colors
            mNumberColors = countDistinctColors(pixels);

            // Create arrays
            mColors = new Color[mNumberColors];
            mColorCounts = new int[mNumberColors];

            // Finally count the frequency of each color
            countFrequencies(pixels);
        }

        public int getNumberOfColors()
        {
            return mNumberColors;
        }

        public Color[] getColors()
        {
            return mColors;
        }

        public int[] getColorCounts()
        {
            return mColorCounts;
        }

        private static int countDistinctColors(Color[] pixels)
        {
            if (pixels.Length < 2)
            {
                // If we have less than 2 pixels we can stop here
                return pixels.Length;
            }

            // If we have at least 2 pixels, we have a minimum of 1 color...
            int colorCount = 1;
            Color currentColor = pixels[0];

            // Now iterate from the second pixel to the end, counting distinct colors
            for (int i = 1; i < pixels.Length; i++)
            {
                // If we encounter a new color, increase the population
                if (pixels[i] != currentColor)
                {
                    currentColor = pixels[i];
                    colorCount++;
                }
            }

            return colorCount;
        }

        private void countFrequencies(Color[] pixels)
        {
            if (pixels.Length == 0)
            {
                return;
            }

            int currentColorIndex = 0;
            Color currentColor = pixels[0];

            mColors[currentColorIndex] = currentColor;
            mColorCounts[currentColorIndex] = 1;

            if (pixels.Length == 1)
            {
                // If we only have one pixel, we can stop here
                return;
            }

            // Now iterate from the second pixel to the end, population distinct colors
            for (int i = 1; i < pixels.Length; i++)
            {
                if (pixels[i] == currentColor)
                {
                    // We've hit the same color as before, increase population
                    mColorCounts[currentColorIndex]++;
                }
                else
                {
                    // We've hit a new color, increase index
                    currentColor = pixels[i];

                    currentColorIndex++;
                    mColors[currentColorIndex] = currentColor;
                    mColorCounts[currentColorIndex] = 1;
                }
            }
        }
    }
}
