using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace com.aurora.aumusic
{
    public class DurationValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan)
            {
                TimeSpan timeSpan = (TimeSpan)value;
                int i = (timeSpan.Days * 24 + timeSpan.Hours) * 60 + timeSpan.Minutes;
                if (timeSpan.Seconds >= 10)
                {
                    return i + ":" + timeSpan.Seconds;
                }
                else
                {
                    return i + ":0" + timeSpan.Seconds;
                }
            }
            else return null;
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

    public class SongsCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string s = value.ToString() + "首曲目";
            return s;
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class ArtistsCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string[] s = (string[])value;
            if (s[0] == "Unknown AlbumArtists")
            {
                return "未知创作人";
            }
            else
            {
                return s.Length + "位创作人";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class AlbumDetailsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            AlbumItem album = (AlbumItem)value;
            if (album == null)
                return null;
            StringBuilder sb = new StringBuilder("由");
            if (album.AlbumArtists[0] == "Unknown AlbumArtists")
            {
                sb.Clear();
                sb.Append("未知创作人");
            }
            else
            {
                int i = 0;
                foreach (var item in album.AlbumArtists)
                {
                    i++;
                    if (i > 2)
                    {
                        sb.Remove(sb.Length - 2, 2);
                        sb.Append("等");
                        break;
                    }
                    sb.Append(item);
                    sb.Append(", ");
                }
                if (sb[sb.Length - 1] == ' ')
                    sb.Remove(sb.Length - 2, 2);
                sb.Append("创作");
            }
            sb.Append("   ");
            TimeSpan ts = new TimeSpan();
            foreach (var item in album.Songs)
            {
                ts += item.Duration;
            }

            sb.Append((((ts.Days) * 24 + ts.Hours * 60) + ts.Minutes).ToString() + "分" + ts.Seconds.ToString() + "秒");
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class ThumbToolTipConveter : IValueConverter
    {
        public double sParmeter = 0;
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
            {
                TimeSpan ts = TimeSpan.FromSeconds(((double)value / 100.0) * sParmeter);
                if (ts.Seconds >= 10)
                {
                    return (((ts.Days) * 24 + ts.Hours) * 60 + ts.Minutes).ToString() + ":" + ts.Seconds;
                }
                return (((ts.Days) * 24 + ts.Hours) * 60 + ts.Minutes).ToString() + ":0" + ts.Seconds;
            }
            return "0:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ScrollParallaxConverter : IValueConverter
    {
        private const double factor = 0.5;
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
            {
                return (double)value * factor;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class MainColorConverter : IValueConverter
    {
        const double r_factor = 0.299, g_factor = 0.587, b_factor = 0.114;
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if(value is SolidColorBrush)
            {
                SolidColorBrush brush = (SolidColorBrush)value;
                Color c = brush.Color;
                if ((c.R * r_factor + c.G * g_factor + c.B * b_factor) < 85)
                {
                    return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
                else return new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class SubColorConverter : IValueConverter
    {
        const double r_factor = 0.299, g_factor = 0.587, b_factor = 0.114;
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is SolidColorBrush)
            {
                SolidColorBrush brush = (SolidColorBrush)value;
                Color c = brush.Color;
                if ((c.R * r_factor + c.G * g_factor + c.B * b_factor) < 85)
                {
                    return new SolidColorBrush(Color.FromArgb(255, 217, 217, 217));
                }
                else return new SolidColorBrush(Color.FromArgb(255, 95, 95, 95));
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
