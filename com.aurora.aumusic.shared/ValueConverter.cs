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
    public class DurationValueConverter : IValueConverter
    {
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

    public class TextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Color color = (Color)value;

            if ((color.R * 0.299 + color.G * 0.587 + color.B * 0.114) < 144)
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

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class ArtistsCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
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

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class UriConverter : IValueConverter
    {
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

    public class AlbumDetailsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            AlbumItem album = (AlbumItem)value;
            if (album == null)
                return null;
            StringBuilder sb = new StringBuilder();
            if (album.AlbumArtists[0] == "Unknown AlbumArtists")
            {
                sb.Clear();
                sb.Append(loader.GetString("UnknownAlbumArtists"));
            }
            else
            {
                if (album.AlbumArtists.Length > 2)
                {
                    sb.Clear();
                    sb.AppendFormat(loader.GetString("AlbumDetailsArtistsShort"), album.AlbumArtists[0], album.AlbumArtists[1]);
                }
                else
                {
                    sb.Clear();
                    sb.Append(loader.GetString("AlbumDetailsArtistsLongStart"));
                    foreach (var artist in album.AlbumArtists)
                    {
                        sb.Append(artist);
                        sb.Append(", ");
                    }
                    sb.Remove(sb.Length - 2, 2);
                    sb.Append(loader.GetString("AlbumDetailsArtistsLongEnd"));
                }
            }
            sb.Append(" ");
            TimeSpan ts = new TimeSpan();
            foreach (var item in album.Songs)
            {
                ts += item.Duration;
            }
            sb.AppendFormat(loader.GetString("AlbumDetailsTotalDuration"), (((ts.Days) * 24 + ts.Hours * 60) + ts.Minutes).ToString("0"), ts.Seconds.ToString("#0"));
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class ArtistDetailsConverter : IValueConverter
    {
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

    public class ArtistsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool shortcount = true;
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            if (parameter is bool)
                if (((bool)parameter) == true)
                    shortcount = false;
                else if (parameter is string)
                    if ((string)parameter == "true")
                        shortcount = false;
            var artists = value as string[];
            StringBuilder sb = new StringBuilder();
            if (artists == null || artists[0] == "Unknown AlbumArtists")
            {
                sb.Clear();
                sb.Append(loader.GetString("UnknownAlbumArtists"));
            }
            else
            {
                if (artists.Length > 2 && shortcount)
                {
                    sb.Clear();
                    sb.AppendFormat(loader.GetString("AlbumDetailsArtistsShort"), artists[0], artists[1]);
                }
                else
                {
                    sb.Clear();
                    sb.Append(loader.GetString("AlbumDetailsArtistsLongStart"));
                    foreach (var artist in artists)
                    {
                        sb.Append(artist);
                        sb.Append(", ");
                    }
                    sb.Remove(sb.Length - 2, 2);
                    sb.Append(loader.GetString("AlbumDetailsArtistsLongEnd"));
                }
            }
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
                return (((ts.Days) * 24 + ts.Hours) * 60 + ts.Minutes).ToString() + ":" + ts.Seconds.ToString("00");
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



    public class FrameParallaxConverterTwo : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double)
            {
                return 528 - (double)value * 0.5 > 0 ? 528 - (double)value * 0.5 : 0.0;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class ColortoBrushConverter : IValueConverter
    {
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

    public class MainColorConverter : IValueConverter
    {
        const double r_factor = 0.299, g_factor = 0.587, b_factor = 0.114;
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

    public class SubColorConverter : IValueConverter
    {
        const double r_factor = 0.299, g_factor = 0.587, b_factor = 0.114;
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

    public class TranslateYConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return -(((double)value) / 2 - 152);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class FolderPathConverter : IValueConverter
    {
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

    public class TotalGenresConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string[])
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                StringBuilder sb = new StringBuilder();
                if (((string[])value) != null && ((string[])value).Length > 0)
                {
                    if (((string[])value)[0] == "Unknown Genres")
                        sb.Append(loader.GetString("UnknownGenres"));
                    else
                        foreach (var item in ((string[])value))
                        {
                            sb.Append(item);
                            sb.Append(", ");
                        }
                    if (sb[sb.Length - 1] == ' ')
                        sb.Remove(sb.Length - 2, 2);
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

    public class TrackDiscConverter : IValueConverter
    {
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

    public class RatingConverter : IValueConverter
    {
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

    public class FileTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                switch ((string)value)
                {
                    case ".mp3": return "MPEG-Layer 3";
                    case ".m4a": return "aac/alac";
                    case ".wav": return "WAVE";
                    case ".flac": return "FLAC";
                    default: return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class FileSizeConverter : IValueConverter
    {
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

    public class TotalDurationConverter : IValueConverter
    {
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

    public class BitRateConverter : IValueConverter
    {
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

    public class ShuffleListArtworkConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is List<Song>)
            {
                var s = value as List<Song>;

            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }

    public class SongsDetailsConverter : IValueConverter
    {
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

    public class GenresDetailsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is AlbumItem)
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                StringBuilder sb = new StringBuilder();
                if (((AlbumItem)value).Genres != null && ((AlbumItem)value).Genres.Length > 0)
                {
                    if (((AlbumItem)value).Genres[0] == "Unknown Genres")
                        sb.Append(loader.GetString("UnknownGenres"));
                    else
                        foreach (var item in ((AlbumItem)value).Genres)
                        {
                            sb.Append(item);
                            sb.Append(", ");
                        }
                    if (sb[sb.Length - 1] == ' ')
                        sb.Remove(sb.Length - 2, 2);
                    sb.Append(" · ");
                }
                if (((AlbumItem)value).Year > 0)
                    sb.AppendFormat(loader.GetString("YearConverter"), ((AlbumItem)value).Year);
                else
                    sb.Remove(sb.Length - 3, 3);
                return sb.ToString();
            }
            if (value is Song)
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                StringBuilder sb = new StringBuilder();
                if (((Song)value).Genres != null && ((Song)value).Genres.Length > 0)
                {
                    if (((Song)value).Genres[0] == "Unknown Genres")
                        sb.Append(loader.GetString("UnknownGenres"));
                    else
                        foreach (var item in ((Song)value).Genres)
                        {
                            sb.Append(item);
                            sb.Append(", ");
                        }
                    if (sb[sb.Length - 1] == ' ')
                        sb.Remove(sb.Length - 2, 2);
                    sb.Append(" · ");
                }
                if (((Song)value).Year > 0)
                    sb.AppendFormat(loader.GetString("YearConverter"), ((Song)value).Year);
                else
                    sb.Remove(sb.Length - 3, 3);
                return sb.ToString();
            }
            if (value is SongModel)
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                StringBuilder sb = new StringBuilder();
                if (((SongModel)value).Genres != null && ((SongModel)value).Genres.Length > 0)
                {
                    if (((SongModel)value).Genres[0] == "Unknown Genres")
                        sb.Append(loader.GetString("UnknownGenres"));
                    else
                        foreach (var item in ((SongModel)value).Genres)
                        {
                            sb.Append(item);
                            sb.Append(", ");
                        }
                    if (sb[sb.Length - 1] == ' ')
                        sb.Remove(sb.Length - 2, 2);
                    sb.Append(" · ");
                }
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
            throw new NotImplementedException();
        }
    }

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
