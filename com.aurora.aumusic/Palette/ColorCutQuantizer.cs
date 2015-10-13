using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace KKBOX.Utility
{
    public class ColorCutQuantizer
    {
        private readonly float[] mTempHsl = new float[3];

        private static readonly float BLACK_MAX_LIGHTNESS = 0.05f;
        private static readonly float WHITE_MIN_LIGHTNESS = 0.95f;

        private static readonly Boolean IS_ENABLE_IGNORE = true;

        private const int COMPONENT_RED = -3;
        private const int COMPONENT_GREEN = -2;
        private const int COMPONENT_BLUE = -1;

        private static Color[] mColors;
        private static List<KeyValuePair<Color, Int32>> mColorPopulations;

        private readonly List<Swatch> mQuantizedColors;


        public static async Task<ColorCutQuantizer> fromBitmap(WriteableBitmap bitmap, int maxColors)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            Color[] pixels = new Color[width * height];

            await WriteableBitmapExtension.GetPixels(bitmap, pixels, width, height);

            return new ColorCutQuantizer(new ColorHistogram(pixels), maxColors);
        }


        private ColorCutQuantizer(ColorHistogram colorHistogram, int maxColors)
        {
            if (colorHistogram == null)
            {
                throw new ArgumentNullException("colorHistogram can not be null");
            }
            if (maxColors < 1)
            {
                throw new ArgumentException("maxColors must be 1 or greater");
            }

            int rawColorCount = colorHistogram.getNumberOfColors();
            Color[] rawColors = colorHistogram.getColors();
            int[] rawColorCounts = colorHistogram.getColorCounts();

            // First, lets pack the populations into a SparseIntArray so that they can be easily
            // retrieved without knowing a color's index
            mColorPopulations = new List<KeyValuePair<Color, Int32>>(rawColorCount);
            for (int i = 0; i < rawColors.Length; i++)
            {
                mColorPopulations.Add(new KeyValuePair<Color, Int32>(rawColors[i], rawColorCounts[i]));
            }

            // Now go through all of the colors and keep those which we do not want to ignore
            mColors = new Color[rawColorCount];
            int validColorCount = 0;

            foreach (Color color in rawColors)
            {
                if (!shouldIgnoreColor(color))
                {
                    mColors[validColorCount++] = color;
                }
            }
            if (validColorCount <= maxColors)
            {
                // The image has fewer colors than the maximum requested, so just return the colors
                mQuantizedColors = new List<Swatch>();
                foreach (Color color in mColors)
                {
                    try
                    {
                        mQuantizedColors.Add(new Swatch(color, mColorPopulations.First(kvp => kvp.Key == color).Value));
                    }
                    catch (Exception)
                    {
                    }

                }
            }
            else
            {
                // We need use quantization to reduce the number of colors
                mQuantizedColors = quantizePixels(validColorCount - 1, maxColors);
            }
        }


        /**
 * @return the list of quantized colors
 */
        public List<Swatch> getQuantizedColors()
        {
            return mQuantizedColors;
        }

        private List<Swatch> quantizePixels(int maxColorIndex, int maxColors)
        {
            // Create the priority queue which is sorted by volume descending. This means we always
            // split the largest box in the queue
            Queue<Vbox> pq = new Queue<Vbox>(maxColors);

            // To start, offer a box which contains all of the colors
            pq.Enqueue(new Vbox(0, maxColorIndex));

            // Now go through the boxes, splitting them until we have reached maxColors or there are no
            // more boxes to split
            pq = splitBoxes(pq, maxColors);

            // Finally, return the average colors of the color boxes
            return generateAverageColors(pq);
        }

        /**
         * Iterate through the {@link java.util.Queue}, popping
         * {@link ColorCutQuantizer.Vbox} objects from the queue
         * and splitting them. Once split, the new box and the remaining box are offered back to the
         * queue.
         *
         * @param queue {@link java.util.PriorityQueue} to poll for boxes
         * @param maxSize Maximum amount of boxes to split
         */
        private Queue<Vbox> splitBoxes(Queue<Vbox> queue, int maxSize)
        {
            while (queue.Count < maxSize)
            {
                queue = new Queue<Vbox>(queue.OrderByDescending(v => v.getVolume()));
                Vbox vbox = queue.Dequeue();

                if (vbox != null && vbox.canSplit())
                {
                    // First split the box, and offer the result
                    queue.Enqueue(vbox.splitBox());
                    // Then offer the box back
                    queue.Enqueue(vbox);
                }
                else
                {
                    break;
                }
            }

            queue = new Queue<Vbox>(queue.OrderBy(v => v.getVolume()));
            return queue;
        }

        private List<Swatch> generateAverageColors(IEnumerable<Vbox> vboxes)
        {
            List<Swatch> colors = new List<Swatch>();
            foreach (Vbox vbox in vboxes)
            {
                Swatch color = vbox.getAverageColor();
                if (!shouldIgnoreColor(color))
                {
                    // As we're averaging a color box, we can still get colors which we do not want, so
                    // we check again here
                    colors.Add(color);
                }
            }
            return colors;
        }

        /**
         * Represents a tightly fitting box around a color space.
         */
        private class Vbox
        {
            private int lowerIndex;
            private int upperIndex;

            private int minRed, maxRed;
            private int minGreen, maxGreen;
            private int minBlue, maxBlue;

            public Vbox(int lowerIndex, int upperIndex)
            {
                this.lowerIndex = lowerIndex;
                this.upperIndex = upperIndex;
                fitBox();
            }

            public int getVolume()
            {
                return (maxRed - minRed + 1) * (maxGreen - minGreen + 1) * (maxBlue - minBlue + 1);
            }

            public Boolean canSplit()
            {
                return getColorCount() > 1;
            }

            int getColorCount()
            {
                return upperIndex - lowerIndex;
            }

            /**
             * Recomputes the boundaries of this box to tightly fit the colors within the box.
             */
            void fitBox()
            {
                // Reset the min and max to opposite values
                minRed = minGreen = minBlue = 0xFF;
                maxRed = maxGreen = maxBlue = 0x0;

                for (int i = lowerIndex; i <= upperIndex; i++)
                {
                    Color color = mColors[i];
                    int r = color.R;
                    int g = color.G;
                    int b = color.B;
                    if (r > maxRed)
                    {
                        maxRed = r;
                    }
                    if (r < minRed)
                    {
                        minRed = r;
                    }
                    if (g > maxGreen)
                    {
                        maxGreen = g;
                    }
                    if (g < minGreen)
                    {
                        minGreen = g;
                    }
                    if (b > maxBlue)
                    {
                        maxBlue = b;
                    }
                    if (b < minBlue)
                    {
                        minBlue = b;
                    }
                }
            }

            /**
             * Split this color box at the mid-point along it's longest dimension
             *
             * @return the new ColorBox
             */
            public Vbox splitBox()
            {
                if (!canSplit())
                {
                    throw new InvalidOperationException("Can not split a box with only 1 color");
                }

                // find median along the longest dimension
                int splitPoint = findSplitPoint();

                Vbox newBox = new Vbox(splitPoint + 1, upperIndex);

                // Now change this box's upperIndex and recompute the color boundaries
                upperIndex = splitPoint;
                fitBox();

                return newBox;
            }

            /**
             * @return the dimension which this box is largest in
             */
            int getLongestColorDimension()
            {
                int redLength = maxRed - minRed;
                int greenLength = maxGreen - minGreen;
                int blueLength = maxBlue - minBlue;

                if (redLength >= greenLength && redLength >= blueLength)
                {
                    return COMPONENT_RED;
                }
                else if (greenLength >= redLength && greenLength >= blueLength)
                {
                    return COMPONENT_GREEN;
                }
                else
                {
                    return COMPONENT_BLUE;
                }
            }

            /**
             * Finds the point within this box's lowerIndex and upperIndex index of where to split.
             *
             * This is calculated by finding the longest color dimension, and then sorting the
             * sub-array based on that dimension value in each color. The colors are then iterated over
             * until a color is found with at least the midpoint of the whole box's dimension midpoint.
             *
             * @return the index of the colors array to split from
             */
            public int findSplitPoint()
            {
                int longestDimension = getLongestColorDimension();

                IComparer<Color> colorComparer = null;
                switch (longestDimension)
                {
                    case COMPONENT_RED:
                        colorComparer = new ColorComparer();
                        break;
                    case COMPONENT_GREEN:
                        colorComparer = new ColorComparerByGreen();
                        break;
                    case COMPONENT_BLUE:
                        colorComparer = new ColorComparerByBlue();
                        break;
                }

                Array.Sort(mColors, lowerIndex, upperIndex - lowerIndex, colorComparer);

                int dimensionMidPoint = midPoint(longestDimension);

                for (int i = lowerIndex; i < upperIndex; i++)
                {
                    Color color = mColors[i];

                    switch (longestDimension)
                    {
                        case COMPONENT_RED:
                            if (color.R >= dimensionMidPoint)
                            {
                                return i;
                            }
                            break;
                        case COMPONENT_GREEN:
                            if (color.G >= dimensionMidPoint)
                            {
                                return i;
                            }
                            break;
                        case COMPONENT_BLUE:
                            if (color.B >= dimensionMidPoint)
                            {
                                return i;
                            }
                            break;
                    }
                }

                return lowerIndex;
            }

            /**
             * @return the average color of this box.
             */
            public Swatch getAverageColor()
            {
                int redSum = 0;
                int greenSum = 0;
                int blueSum = 0;
                int totalPopulation = 0;

                for (int i = lowerIndex; i <= upperIndex; i++)
                {
                    Color color = mColors[i];
                    var targetColor = mColorPopulations.First(kvp => kvp.Key == color);
                    int colorPopulation = targetColor.Value;

                    totalPopulation += colorPopulation;
                    redSum += colorPopulation * color.R;
                    greenSum += colorPopulation * color.G;
                    blueSum += colorPopulation * color.B;
                }

                int redAverage = (Int32)Math.Round(redSum / (float)totalPopulation);
                int greenAverage = (Int32)Math.Round(greenSum / (float)totalPopulation);
                int blueAverage = (Int32)Math.Round(blueSum / (float)totalPopulation);

                return new Swatch(redAverage, greenAverage, blueAverage, totalPopulation);
            }

            /**
             * @return the midpoint of this box in the given {@code dimension}
             */
            public int midPoint(int dimension)
            {
                switch (dimension)
                {
                    case COMPONENT_RED:
                    default:
                        return (minRed + maxRed) / 2;
                    case COMPONENT_GREEN:
                        return (minGreen + maxGreen) / 2;
                    case COMPONENT_BLUE:
                        return (minBlue + maxBlue) / 2;
                }
            }
        }

        private Boolean shouldIgnoreColor(Color color)
        {
            ColorUtils.RGBtoHSL(color.R, color.G, color.B, mTempHsl);
            return shouldIgnoreColor(mTempHsl);
        }

        private static Boolean shouldIgnoreColor(Swatch color)
        {
            return shouldIgnoreColor(color.getHsl());
        }

        private static Boolean shouldIgnoreColor(float[] hslColor)
        {
            return IS_ENABLE_IGNORE && (isWhite(hslColor) || isBlack(hslColor) || isNearRedILine(hslColor));
        }

        /**
         * @return true if the color represents a color which is close to black.
         */
        private static Boolean isBlack(float[] hslColor)
        {
            return hslColor[2] <= BLACK_MAX_LIGHTNESS;
        }

        /**
         * @return true if the color represents a color which is close to white.
         */
        private static Boolean isWhite(float[] hslColor)
        {
            return hslColor[2] >= WHITE_MIN_LIGHTNESS;
        }

        /**
         * @return true if the color lies close to the red side of the I line.
         */
        private static Boolean isNearRedILine(float[] hslColor)
        {
            return hslColor[0] >= 10f && hslColor[0] <= 37f && hslColor[1] <= 0.82f;
        }
    }
}
