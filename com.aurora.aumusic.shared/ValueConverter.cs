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
using com.aurora.aumusic.shared.Albums;
using com.aurora.aumusic.shared.Helpers;
using com.aurora.aumusic.shared.Songs;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace com.aurora.aumusic.shared
{
    /// <summary>
    /// Convert TimeSpan to a readable format like "3:05"
    /// </summary>
    public class DurationValueConverter : IValueConverter
    {
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">Value must be TimeSpan, else return null</param>
        /// <param name="targetType">null</param>
        /// <param name="parameter">null</param>
        /// <param name="language">null</param>
        /// <returns>TotalMinutes : Seconds</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan)
            {
                TimeSpan timeSpan = (TimeSpan)value;
                int i = (timeSpan.Days * 24 + timeSpan.Hours) * 60 + timeSpan.Minutes;
                return i + ":" + timeSpan.Seconds.ToString("00");
            }
            else return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// Convert songs count value to readable format based on language
    /// </summary>
    public class SongsCountConverter : IValueConverter
    {
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">value must be int, else return blank</param>
        /// <param name="targetType">null</param>
        /// <param name="parameter">null</param>
        /// <param name="language">null</param>
        /// <returns>as "X Song(s)"</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int)
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                string str;
                if ((int)value == 1)
                {
                    str = loader.GetString("SongsCountConverterSingle");
                }
                else
                {
                    str = loader.GetString("SongsCountConverter");
                }
                string s = string.Format(str, value);
                return s;
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// Convert Aritsts count to readable format based on language
    /// </summary>
    public class ArtistsCountConverter : IValueConverter
    {
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">value must be string[], else return blank</param>
        /// <param name="targetType">null</param>
        /// <param name="parameter">null</param>
        /// <param name="language">null</param>
        /// <returns>as X AlbumArtist(s)</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string[])
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                string[] s = (string[])value;
                if (s[0] == "Unknown AlbumArtists")
                {
                    var str = loader.GetString("UnknownAlbumArtists");
                    return str;
                }
                else
                {
                    if (s.Length == 1)
                    {
                        var str = loader.GetString("AlbumArtistsSingle");
                        return string.Format(str, s.Length);
                    }
                    else
                    {
                        var str = loader.GetString("AlbumArtists");
                        return string.Format(str, s.Length);
                    }
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// Convert string to Uri
    /// </summary>
    public class UriConverter : IValueConverter
    {
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">value must be string and can convert directly, else return null</param>
        /// <param name="targetType">null</param>
        /// <param name="parameter">null</param>
        /// <param name="language">null</param>
        /// <returns>return new Uri(value)</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                return new Uri((string)value);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convert album to readable details(Artists and Total Duration)
    /// </summary>
    public class AlbumDetailsConverter : IValueConverter
    {
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">value must be AlbumItem, else return null</param>
        /// <param name="targetType">null</param>
        /// <param name="parameter">null</param>
        /// <param name="language">null</param>
        /// <returns>"Created by A B... and X:0X"</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is AlbumItem)
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                AlbumItem album = (AlbumItem)value;
                if (album == null)
                    return null;
                var sb = ArtistsConverter.ArtiststoString(false, album.AlbumArtists);

                sb.Append(" · ");
                TimeSpan ts = new TimeSpan();
                foreach (var item in album.Songs)
                {
                    ts += item.Duration;
                }
                sb.AppendFormat(loader.GetString("AlbumDetailsTotalDuration"), (((ts.Days) * 24 + ts.Hours * 60) + ts.Minutes).ToString("0"), ts.Seconds.ToString("0#"));
                return sb.ToString();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// in ArtistsPage, convert Artists albums and songs count to readable format
    /// </summary>
    public class ArtistDetailsConverter : IValueConverter
    {
        /// <summary>
        /// Conver method
        /// </summary>
        /// <param name="value">value must be AlbumSongsGroup List, else return null</param>
        /// <param name="targetType">null</param>
        /// <param name="parameter">null</param>
        /// <param name="language">null</param>
        /// <returns>"X Album(s) X Song(s)"</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is List<AlbumSongsGroup>)
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                StringBuilder sb = new StringBuilder();
                int i = 0;
                foreach (var item in (List<AlbumSongsGroup>)value)
                {
                    i += item.SongsCount;
                }
                if (((List<AlbumSongsGroup>)value).Count == 1)
                {
                    sb.AppendFormat(loader.GetString("ArtistDetailsConverterAlbumSingle"), ((List<AlbumSongsGroup>)value).Count.ToString("0"));
                }
                else
                {
                    sb.AppendFormat(loader.GetString("ArtistDetailsConverterAlbum"), ((List<AlbumSongsGroup>)value).Count.ToString("0"));
                }
                if (i == 1)
                {
                    sb.AppendFormat(loader.GetString("ArtistDetailsConverterSongsSingle"), i.ToString("0"));
                }
                else
                {
                    sb.AppendFormat(loader.GetString("ArtistDetailsConverterSongs"), i.ToString("0"));
                }
                return sb.ToString();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// Convert string[] artists to readable string
    /// </summary>
    public class ArtistsConverter : IValueConverter
    {
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">value must be string[], else return null</param>
        /// <param name="targetType">null</param>
        /// <param name="parameter">true or "ture" means totally convert all the artists</param>
        /// <param name="language">null</param>
        /// <returns>"Created by A, B, C..."</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string[])
            {
                bool LongFormat = false;

                if (parameter is bool)
                    if (((bool)parameter) == true)
                        LongFormat = true;
                    else if (parameter is string)
                        if ((string)parameter == "true")
                            LongFormat = true;
                var artists = value as string[];
                return ArtiststoString(LongFormat, artists).ToString();
            }
            return null;
        }

        /// <summary>
        /// Artists to string module
        /// </summary>
        /// <param name="longformat">true means totally convert all artists</param>
        /// <param name="artists">artist list</param>
        /// <returns>a stringbuilder contains "Created by A, B, C..."</returns>
        public static StringBuilder ArtiststoString(bool longformat, string[] artists)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            StringBuilder sb = new StringBuilder();
            if (artists == null || artists[0] == "Unknown AlbumArtists")
            {
                sb.Clear();
                sb.Append(loader.GetString("UnknownAlbumArtists"));
            }
            else
            {
                if (artists.Length > 2 && !longformat)
                {
                    sb.Clear();
                    sb.AppendFormat(loader.GetString("AlbumDetailsArtistsShort"), artists[0], artists[1]);
                }
                else
                {
                    sb.Clear();
                    var tempsb = new StringBuilder();
                    foreach (var artist in artists)
                    {
                        tempsb.Append(artist);
                        tempsb.Append(", ");
                    }
                    tempsb.Remove(tempsb.Length - 2, 2);
                    sb.AppendFormat(loader.GetString("AlbumDetailsArtistsLong"), tempsb.ToString());
                    tempsb.Clear();
                    tempsb = null;
                }
            }
            return sb;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// Convert the Slider thumbtooltip to a time format
    /// </summary>
    public class ThumbToolTipConveter : IValueConverter
    {
        /// <summary>
        /// sParameter stores the TimeSpan's TotalSeconds of current song.(Thumbtooltip can't pass converter parameter)
        /// </summary>
        public double sParameter = 0;
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">value must be TimeSpan's TotalSeconds</param>
        /// <param name="targetType">null</param>
        /// <param name="parameter">null</param>
        /// <param name="language">null</param>
        /// <returns>return "X:0X", based on the total duration and slider value</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
            {
                TimeSpan ts = TimeSpan.FromSeconds(((double)value / 100.0) * sParameter);
                return (((ts.Days) * 24 + ts.Hours) * 60 + ts.Minutes).ToString() + ":" + ts.Seconds.ToString("00");
            }
            return "0:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Parallax Scroll Helper with changing RenderTramsform, default parallax factor is 0.5
    /// </summary>
    public class ScrollParallaxConverter : IValueConverter
    {
        /// <summary>
        /// default parallax factor, change in the Convert method
        /// </summary>
        private static double factor = 0.5;
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">Scroller's vertical offset, must be double</param>
        /// <param name="targetType">null</param>
        /// <param name="parameter">pass a double value change the parallax factor</param>
        /// <param name="language">null</param>
        /// <returns>RenderTransform.TranslateY</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
            {
                factor = parameter != null ? (double)parameter : 0.5;
                return (double)value * factor;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }


    /// <summary>
    /// Parallax scroll helper with changing control's height, default parallax factor is 0.5
    /// </summary>
    public class FrameParallaxConverter : IValueConverter
    {
        /// <summary>
        /// default parallax factor, change in the Convert method
        /// </summary>
        private static double factor = 0.5;
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">Scroller's vertical offset, must be double</param>
        /// <param name="targetType">null</param>
        /// <param name="parameter">pass a double value change the parallax factor</param>
        /// <param name="language">null</param>
        /// <returns>Control's Height</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
            {
                factor = parameter != null ? (double)parameter : 0.5;
                return 528 - (double)value * 0.5 > 0 ? 528 - (double)value * 0.5 : 0.0;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// Convert Color to SolidColorBrush
    /// </summary>
    public class ColortoBrushConverter : IValueConverter
    {
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">value must be Color, else return null</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns>SolidColorBrush</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Color)
            {
                return new SolidColorBrush((Color)value);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// Convert Main Text Color Based on Background Color
    /// </summary>
    public class MainColorConverter : IValueConverter
    {
        const double r_factor = 0.299, g_factor = 0.587, b_factor = 0.114;
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">value must be Color, else return black</param>
        /// <param name="targetType">null</param>
        /// <param name="parameter">null</param>
        /// <param name="language">null</param>
        /// <returns>if luma &lt; 0.56, return white, else return black</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is SolidColorBrush)
            {
                SolidColorBrush brush = (SolidColorBrush)value;
                Color c = brush.Color;
                if ((c.R * r_factor + c.G * g_factor + c.B * b_factor) < 144)
                {
                    return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                }
                else return new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            }
            if (value is Color)
            {
                var c = (Color)value;
                if ((c.R * r_factor + c.G * g_factor + c.B * b_factor) < 144)
                {
                    return Color.FromArgb(255, 255, 255, 255);
                }
                else return Color.FromArgb(255, 0, 0, 0);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convert Sub Text Color Based on Background Color
    /// </summary>
    public class SubColorConverter : IValueConverter
    {
        const double r_factor = 0.299, g_factor = 0.587, b_factor = 0.114;
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">value must be Color, else return black</param>
        /// <param name="targetType">null</param>
        /// <param name="parameter">null</param>
        /// <param name="language">null</param>
        /// <returns>if luma &lt; 0.56, return graywhite, else return grayblack</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is SolidColorBrush)
            {
                SolidColorBrush brush = (SolidColorBrush)value;
                Color c = brush.Color;
                if ((c.R * r_factor + c.G * g_factor + c.B * b_factor) < 144)
                {
                    return new SolidColorBrush(Color.FromArgb(255, 240, 240, 240));
                }
                else return new SolidColorBrush(Color.FromArgb(255, 35, 35, 35));
            }
            if (value is Color)
            {
                var c = (Color)value;
                if ((c.R * r_factor + c.G * g_factor + c.B * b_factor) < 144)
                {
                    return Color.FromArgb(255, 240, 240, 240);
                }
                else return Color.FromArgb(255, 35, 35, 35);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convert StorageFolder to string path
    /// </summary>
    public class FolderPathConverter : IValueConverter
    {
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">value must be StorageFolder, else return null</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns>value's FolderPath as string</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is StorageFolder)
            {
                return (value as StorageFolder).Path;
            }
            else return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// trim the path, replace "\\" to "\"
    /// </summary>
    public class StringPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                return ((string)value).Replace("\\\\", "\\");
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// convert uint to string year "####"
    /// </summary>
    public class YearConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is uint)
            {
                return ((uint)value).ToString("####");
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// Convert string[] Genres to readable string based on language
    /// </summary>
    public class TotalGenresConverter : IValueConverter
    {
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">value must be string[], else return null</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns>"A,B..."</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string[])
            {
                return GenrestoString((string[])value).ToString();
            }
            return null;
        }

        /// <summary>
        /// Convert string[] Genres to string
        /// </summary>
        /// <param name="value">value must be string[]</param>
        /// <returns>stringbuilder contains "A, B, C..."</returns>
        public static StringBuilder GenrestoString(string[] value)
        {
            var sb = new StringBuilder();
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            if (value != null && (value).Length > 0)
            {
                if ((value)[0] == "Unknown Genres")
                    sb.Append(loader.GetString("UnknownGenres"));
                else
                    foreach (var item in value)
                    {
                        sb.Append(item);
                        sb.Append(", ");
                    }
                if (sb[sb.Length - 1] == ' ')
                    sb.Remove(sb.Length - 2, 2);
            }
            return sb;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// Convert Track and Disc to string
    /// </summary>
    public class TrackDiscConverter : IValueConverter
    {
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">value must be uint[track,trackcount,disc,disccount]</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns>"2/10  1/2"</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is uint[])
            {
                var tracks = value as uint[];
                return tracks[0].ToString("0") + "/" + tracks[1].ToString("0") + "  " + tracks[2].ToString("0") + "/" + tracks[3].ToString("0");
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// Convert Rating to Stars string
    /// </summary>
    public class RatingConverter : IValueConverter
    {
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">value must be uint and &lt; 5</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns>Stars(display with segoe symbol ui)</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is uint)
            {
                switch ((uint)value)
                {
                    case 0: return "\xE224\xE224\xE224\xE224\xE224";
                    case 1: return "\xE0B4\xE224\xE224\xE224\xE224";
                    case 2: return "\xE0B4\xE0B4\xE224\xE224\xE224";
                    case 3: return "\xE0B4\xE0B4\xE0B4\xE224\xE224";
                    case 4: return "\xE0B4\xE0B4\xE0B4\xE0B4\xE224";
                    case 5: return "\xE0B4\xE0B4\xE0B4\xE0B4\xE0B4";
                    default: return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// Guess the decoder based on AudioFile type
    /// </summary>
    public class FileTypeConverter : IValueConverter
    {
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">value must be string from".mp3, .m4a, .wav, .flac"</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                switch ((string)value)
                {
                    case ".mp3": return "MPEG-Layer 3";
                    case ".m4a": return "Advanced Audio Codec/Apple Lossless";
                    case ".wav": return "Waveform Audio";
                    case ".flac": return "Free Lossless Audio Codec";
                    default: return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// return ulong size to 'XX'MB
    /// </summary>
    public class FileSizeConverter : IValueConverter
    {
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">value must be ulong</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns>'XX'MB</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ulong)
            {
                return ((((ulong)value) / 1024.00) / 1024.00).ToString("0.##") + "MB";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// convert file's duration "X:0X.00"
    /// </summary>
    public class TotalDurationConverter : IValueConverter
    {
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">value must be TimeSpan</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns>"X:0X.00"</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TimeSpan)
            {
                TimeSpan timeSpan = (TimeSpan)value;
                int i = (timeSpan.Days * 24 + timeSpan.Hours) * 60 + timeSpan.Minutes;
                return i + ":" + timeSpan.Seconds.ToString("00") + "." + timeSpan.Milliseconds.ToString("00");
            }
            else return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// convert uint bitrate to "0.XX Kbps"
    /// </summary>
    public class BitRateConverter : IValueConverter
    {
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">value must be uint</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns>"0.XX'Kbps</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is uint)
            {
                return ((uint)value / 1000.00).ToString("0.##") + "Kbps";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// convert Songs to "Play X Song(s)"
    /// </summary>
    public class SongsDetailsConverter : IValueConverter
    {
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">value must be AlbumItem</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns>"Play X Song(s)"</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is AlbumItem)
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                StringBuilder sb = new StringBuilder();
                if (((AlbumItem)value).SongsCount == 1)
                {
                    sb.AppendFormat(loader.GetString("SongsDetailsConverterSingle"), ((AlbumItem)value).SongsCount);
                }
                else
                    sb.AppendFormat(loader.GetString("SongsDetailsConverter"), ((AlbumItem)value).SongsCount);
                return sb.ToString();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// Convert Genres and Year to string
    /// </summary>
    public class GenresDetailsConverter : IValueConverter
    {
        /// <summary>
        /// Convert Method
        /// </summary>
        /// <param name="value">AlbumItem or Song or SongModel</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns>"A,B,C...  Year XXXX"</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is AlbumItem)
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                var sb = TotalGenresConverter.GenrestoString(((AlbumItem)value).Genres);

                sb.Append(" · ");
                if (((AlbumItem)value).Year > 0)
                    sb.AppendFormat(loader.GetString("YearConverter"), ((AlbumItem)value).Year);
                else
                    sb.Remove(sb.Length - 3, 3);
                return sb.ToString();
            }
            if (value is Song)
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                var sb = TotalGenresConverter.GenrestoString(((Song)value).Genres);

                sb.Append(" · ");
                if (((Song)value).Year > 0)
                    sb.AppendFormat(loader.GetString("YearConverter"), ((Song)value).Year);
                else
                    sb.Remove(sb.Length - 3, 3);
                return sb.ToString();
            }
            if (value is SongModel)
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                var sb = TotalGenresConverter.GenrestoString(((SongModel)value).Genres);

                sb.Append(" · ");
                if (((SongModel)value).Year > 0)
                    sb.AppendFormat(loader.GetString("YearConverter"), ((SongModel)value).Year);
                else
                    sb.Remove(sb.Length - 3, 3);
                return sb.ToString();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// Convert Volume Button Icon Based on Volume 
    /// </summary>
    public class VolumeButtonConverter : IValueConverter
    {
        BitmapIcon vol_low = new BitmapIcon(), vol_mid = new BitmapIcon(), vol_mute = new BitmapIcon(), vol_high = new BitmapIcon(), vol_no = new BitmapIcon();
        public VolumeButtonConverter()
        {
            vol_mid.UriSource = new Uri("ms-appx:///Assets/ButtonIcon/volume-mid.png");
            vol_low.UriSource = new Uri("ms-appx:///Assets/ButtonIcon/volume-low.png");
            vol_mute.UriSource = new Uri("ms-appx:///Assets/ButtonIcon/volume-mute.png");
            vol_high.UriSource = new Uri("ms-appx:///Assets/ButtonIcon/volume-high.png");
            vol_no.UriSource = new Uri("ms-appx:///Assets/ButtonIcon/volume-no.png");
        }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((double)value == 0)
            {
                return vol_no;
            }
            else if ((double)value < 0.2)
            {
                return vol_mute;
            }
            else if ((double)value < 0.5)
            {
                return vol_low;
            }
            else if ((double)value < 0.8)
            {
                return vol_mid;
            }
            else return vol_high;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    /// <summary>
    /// 已弃用，不更改 AppTheme 的情况下，无法动态从 light 和 dark 中获取 ThemeResources
    /// </summary>
    public class ListViewItemStyleSelector : StyleSelector
    {
        protected override Style SelectStyleCore(object item,
            DependencyObject container)
        {
            Style st = new Style();
            st.TargetType = typeof(ListViewItem);
            Setter backGroundSetter = new Setter();
            backGroundSetter.Property = ListViewItem.BackgroundProperty;
            ListView listView =
                ItemsControl.ItemsControlFromItemContainer(container)
                  as ListView;
            int index =
                listView.IndexFromContainer(container);
            if (index % 2 == 0)
            {
                backGroundSetter.Value = (Color)Application.Current.Resources["SystemBackgroundAltHighColor"];
            }
            else
            {
                backGroundSetter.Value = (Color)Application.Current.Resources["SystemAltHighColor"];
            }
            st.Setters.Add(backGroundSetter);
            Setter paddingSetter = new Setter();
            paddingSetter.Property = ListViewItem.PaddingProperty;
            paddingSetter.Value = 0;
            st.Setters.Add(paddingSetter);
            Setter alignSetter = new Setter();
            alignSetter.Property = ListViewItem.HorizontalContentAlignmentProperty;
            alignSetter.Value = "Stretch";
            st.Setters.Add(alignSetter);
            return st;
        }
    }
}
