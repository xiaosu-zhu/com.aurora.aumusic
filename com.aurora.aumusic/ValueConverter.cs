using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;

namespace com.aurora.aumusic
{
    public class DurationValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            TimeSpan timeSpan = (TimeSpan)value;
            int i = timeSpan.Hours * 60 + timeSpan.Minutes;
            string s;
            if (timeSpan.Seconds >= 10)
            {
                s = i + ":" + timeSpan.Seconds;
            }
            else
            {
                s = i + ":0" + timeSpan.Seconds;
            }
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class TextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Color color = (Color)value;
            
            if ((color.R * 0.299 + color.G * 0.587 + color.B * 0.114) < 85)
            {
                byte b = (byte)(255 - (int)parameter);
                return Color.FromArgb(255, b, b, b);
            }
            else
            {
                byte b = (byte)parameter;
                return Color.FromArgb(255, b, b, b);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
