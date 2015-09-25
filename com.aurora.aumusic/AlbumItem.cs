using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic
{
    public class AlbumItem
    {
        private double _height = double.NaN;
        public double Height
        {
            get
            {
                if (double.IsNaN(_height))
                {
                    Random r = new Random();
                    _height = 150 + (r.NextDouble() - 0.5) * 150;
                }
                return _height;
            }
        }

        public string Text
        {
            get; set;
        }
    }
}
