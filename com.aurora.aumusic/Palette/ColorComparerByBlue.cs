using System;
using System.Collections.Generic;
using Windows.UI;

namespace KKBOX.Utility
{
    public class ColorComparerByBlue : IComparer<Color>
    {
        public ColorComparerByBlue() { }

        public int Compare(Color x, Color y)
        {
            Int32 result = 0;

            if (x.B > y.B)
            {
                result = 1;
            }
            else if (x.B < y.B)
            {
                result = -1;
            }
            else if (x.G > y.G)
            {
                result = 1;
            }
            else if (x.G < y.G)
            {
                result = -1;
            }
            else if (x.R > y.R)
            {
                result = 1;
            }
            else if (x.R < y.R)
            {
                result = -1;
            }

            return result;
        }
    }
}
