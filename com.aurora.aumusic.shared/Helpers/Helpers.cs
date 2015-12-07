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
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace com.aurora.aumusic.shared
{
    public static class ApplicationSettingsHelper
    {

        /// <summary>
        /// Function to read a setting value and clear it after reading it
        /// </summary>
        public static object ReadResetSettingsValue(string key)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                return null;
            }
            else
            {
                var value = ApplicationData.Current.LocalSettings.Values[key];
                ApplicationData.Current.LocalSettings.Values.Remove(key);
                return value;
            }
        }

        /// <summary>
        /// Function to read a setting value
        /// </summary>
        public static object ReadSettingsValue(string key)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                return null;
            }
            else
            {
                var value = ApplicationData.Current.LocalSettings.Values[key];
                return value;
            }
        }

        /// <summary>
        /// Save a key value pair in settings. Create if it doesn't exist
        /// </summary>
        public static void SaveSettingsValue(string key, object value)
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(key))
            {
                ApplicationData.Current.LocalSettings.Values.Add(key, value);
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values[key] = value;
            }
        }

        public static void SetAutoTheme(CurrentTheme theme)
        {
            var Now = System.DateTime.UtcNow;
            Now = Now.ToLocalTime();
            if (Now.Hour < 7 || Now.Hour > 20)
            {
                theme.Theme = ElementTheme.Dark;
            }
            else
            {
                theme.Theme = ElementTheme.Light;
            }
        }
    }

    public class CurrentTheme : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private ElementTheme theme;
        public ElementTheme Theme
        {
            get
            {
                return theme;
            }
            set
            {
                theme = value;
                this.OnPropertyChanged();
            }
        }
        public CurrentTheme()
        {
            var str = (string)ApplicationSettingsHelper.ReadSettingsValue("ThemeSettings");
            if (str == null)
            {
                ApplicationSettingsHelper.SetAutoTheme(this);
            }
            else if (str == "auto")
            {
                ApplicationSettingsHelper.SetAutoTheme(this);
            }
            else if (str == "Light")
            {
                this.Theme = ElementTheme.Light;
            }
            else if (str == "Dark")
            {
                this.Theme = ElementTheme.Dark;
            }
        }
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class EnumHelper
    {
        public static T Parse<T>(string value) where T : struct
        {
            return (T)Enum.Parse(typeof(T), value);
        }
    }

    internal sealed class SunTimes
    {
        #region Private Data Members

        private object mLock = new object();

        private const double mDR = Math.PI / 180;
        private const double mK1 = 15 * mDR * 1.0027379;

        private int[] mRiseTimeArr = new int[2] { 0, 0 };
        private int[] mSetTimeArr = new int[2] { 0, 0 };
        private double mRizeAzimuth = 0.0;
        private double mSetAzimuth = 0.0;

        private double[] mSunPositionInSkyArr = new double[2] { 0.0, 0.0 };
        private double[] mRightAscentionArr = new double[3] { 0.0, 0.0, 0.0 };
        private double[] mDecensionArr = new double[3] { 0.0, 0.0, 0.0 };
        private double[] mVHzArr = new double[3] { 0.0, 0.0, 0.0 };

        private bool mIsSunrise = false;
        private bool mIsSunset = false;

        #endregion

        #region Singleton

        private static readonly SunTimes mInstance = new SunTimes();    // The singleton instance

        private SunTimes() { }

        public static SunTimes Instance
        {
            get { return mInstance; }
        }

        #endregion

        internal abstract class Coords
        {
            internal protected int mDegrees = 0;
            internal protected int mMinutes = 0;
            internal protected int mSeconds = 0;

            public double ToDouble()
            {
                return Sign() * (mDegrees + ((double)mMinutes / 60) + ((double)mSeconds / 3600));
            }

            internal protected abstract int Sign();
        }

        public class LatitudeCoords : Coords
        {
            public enum Direction
            {
                North,
                South
            }
            internal protected Direction mDirection = Direction.North;

            public LatitudeCoords(int degrees, int minutes, int seconds, Direction direction)
            {
                mDegrees = degrees;
                mMinutes = minutes;
                mSeconds = seconds;
                mDirection = direction;
            }

            protected internal override int Sign()
            {
                return (mDirection == Direction.North ? 1 : -1);
            }
        }

        public class LongitudeCoords : Coords
        {
            public enum Direction
            {
                East,
                West
            }

            internal protected Direction mDirection = Direction.East;

            public LongitudeCoords(int degrees, int minutes, int seconds, Direction direction)
            {
                mDegrees = degrees;
                mMinutes = minutes;
                mSeconds = seconds;
                mDirection = direction;
            }

            protected internal override int Sign()
            {
                return (mDirection == Direction.East ? 1 : -1);
            }
        }

        /// <summary>
        /// Calculate sunrise and sunset times. Returns false if time zone and longitude are incompatible.
        /// </summary>
        /// <param name="lat">Latitude coordinates.</param>
        /// <param name="lon">Longitude coordinates.</param>
        /// <param name="date">Date for which to calculate.</param>
        /// <param name="riseTime">Sunrise time (output)</param>
        /// <param name="setTime">Sunset time (output)</param>
        /// <param name="isSunrise">Whether or not the sun rises at that day</param>
        /// <param name="isSunset">Whether or not the sun sets at that day</param>
        public bool CalculateSunRiseSetTimes(LatitudeCoords lat, LongitudeCoords lon, DateTime date,
                                                ref DateTime riseTime, ref DateTime setTime,
                                                ref bool isSunrise, ref bool isSunset)
        {
            return CalculateSunRiseSetTimes(lat.ToDouble(), lon.ToDouble(), date, ref riseTime, ref setTime, ref isSunrise, ref isSunset);
        }

        /// <summary>
        /// Calculate sunrise and sunset times. Returns false if time zone and longitude are incompatible.
        /// </summary>
        /// <param name="lat">Latitude in decimal notation.</param>
        /// <param name="lon">Longitude in decimal notation.</param>
        /// <param name="date">Date for which to calculate.</param>
        /// <param name="riseTime">Sunrise time (output)</param>
        /// <param name="setTime">Sunset time (output)</param>
        /// <param name="isSunrise">Whether or not the sun rises at that day</param>
        /// <param name="isSunset">Whether or not the sun sets at that day</param>
        public bool CalculateSunRiseSetTimes(double lat, double lon, DateTime date,
                                                ref DateTime riseTime, ref DateTime setTime,
                                                ref bool isSunrise, ref bool isSunset)
        {
            lock (mLock)    // lock for thread safety
            {
                var timezone = System.TimeZoneInfo.Local;
                double zone = -(int)Math.Round(timezone.BaseUtcOffset.TotalSeconds / 3600);
                double jd = GetJulianDay(date) - 2451545;  // Julian day relative to Jan 1.5, 2000

                if ((Sign(zone) == Sign(lon)) && (zone != 0))
                {
                    return false;
                }

                lon = lon / 360;
                double tz = zone / 24;
                double ct = jd / 36525 + 1;                                 // centuries since 1900.0
                double t0 = LocalSiderealTimeForTimeZone(lon, jd, tz);      // local sidereal time

                // get sun position at start of day
                jd += tz;
                CalculateSunPosition(jd, ct);
                double ra0 = mSunPositionInSkyArr[0];
                double dec0 = mSunPositionInSkyArr[1];

                // get sun position at end of day
                jd += 1;
                CalculateSunPosition(jd, ct);
                double ra1 = mSunPositionInSkyArr[0];
                double dec1 = mSunPositionInSkyArr[1];

                // make continuous 
                if (ra1 < ra0)
                    ra1 += 2 * Math.PI;

                // initialize
                mIsSunrise = false;
                mIsSunset = false;

                mRightAscentionArr[0] = ra0;
                mDecensionArr[0] = dec0;

                // check each hour of this day
                for (int k = 0; k < 24; k++)
                {
                    mRightAscentionArr[2] = ra0 + (k + 1) * (ra1 - ra0) / 24;
                    mDecensionArr[2] = dec0 + (k + 1) * (dec1 - dec0) / 24;
                    mVHzArr[2] = TestHour(k, zone, t0, lat);

                    // advance to next hour
                    mRightAscentionArr[0] = mRightAscentionArr[2];
                    mDecensionArr[0] = mDecensionArr[2];
                    mVHzArr[0] = mVHzArr[2];
                }

                riseTime = new DateTime(date.Year, date.Month, date.Day, mRiseTimeArr[0], mRiseTimeArr[1], 0);
                setTime = new DateTime(date.Year, date.Month, date.Day, mSetTimeArr[0], mSetTimeArr[1], 0);

                isSunset = true;
                isSunrise = true;

                // neither sunrise nor sunset
                if ((!mIsSunrise) && (!mIsSunset))
                {
                    if (mVHzArr[2] < 0)
                        isSunrise = false; // Sun down all day
                    else
                        isSunset = false; // Sun up all day
                }
                // sunrise or sunset
                else
                {
                    if (!mIsSunrise)
                        // No sunrise this date
                        isSunrise = false;
                    else if (!mIsSunset)
                        // No sunset this date
                        isSunset = false;
                }

                return true;
            }
        }

        #region Private Methods

        private int Sign(double value)
        {
            int rv = 0;

            if (value > 0.0) rv = 1;
            else if (value < 0.0) rv = -1;
            else rv = 0;

            return rv;
        }

        // Local Sidereal Time for zone
        private double LocalSiderealTimeForTimeZone(double lon, double jd, double z)
        {
            double s = 24110.5 + 8640184.812999999 * jd / 36525 + 86636.6 * z + 86400 * lon;
            s = s / 86400;
            s = s - Math.Floor(s);
            return s * 360 * mDR;
        }

        // determine Julian day from calendar date
        // (Jean Meeus, "Astronomical Algorithms", Willmann-Bell, 1991)
        private double GetJulianDay(DateTime date)
        {
            int month = date.Month;
            int day = date.Day;
            int year = date.Year;

            bool gregorian = (year < 1583) ? false : true;

            if ((month == 1) || (month == 2))
            {
                year = year - 1;
                month = month + 12;
            }

            double a = Math.Floor((double)year / 100);
            double b = 0;

            if (gregorian)
                b = 2 - a + Math.Floor(a / 4);
            else
                b = 0.0;

            double jd = Math.Floor(365.25 * (year + 4716))
                       + Math.Floor(30.6001 * (month + 1))
                       + day + b - 1524.5;

            return jd;
        }

        // sun's position using fundamental arguments 
        // (Van Flandern & Pulkkinen, 1979)
        private void CalculateSunPosition(double jd, double ct)
        {
            double g, lo, s, u, v, w;

            lo = 0.779072 + 0.00273790931 * jd;
            lo = lo - Math.Floor(lo);
            lo = lo * 2 * Math.PI;

            g = 0.993126 + 0.0027377785 * jd;
            g = g - Math.Floor(g);
            g = g * 2 * Math.PI;

            v = 0.39785 * Math.Sin(lo);
            v = v - 0.01 * Math.Sin(lo - g);
            v = v + 0.00333 * Math.Sin(lo + g);
            v = v - 0.00021 * ct * Math.Sin(lo);

            u = 1 - 0.03349 * Math.Cos(g);
            u = u - 0.00014 * Math.Cos(2 * lo);
            u = u + 0.00008 * Math.Cos(lo);

            w = -0.0001 - 0.04129 * Math.Sin(2 * lo);
            w = w + 0.03211 * Math.Sin(g);
            w = w + 0.00104 * Math.Sin(2 * lo - g);
            w = w - 0.00035 * Math.Sin(2 * lo + g);
            w = w - 0.00008 * ct * Math.Sin(g);

            // compute sun's right ascension
            s = w / Math.Sqrt(u - v * v);
            mSunPositionInSkyArr[0] = lo + Math.Atan(s / Math.Sqrt(1 - s * s));

            // ...and declination 
            s = v / Math.Sqrt(u);
            mSunPositionInSkyArr[1] = Math.Atan(s / Math.Sqrt(1 - s * s));
        }

        // test an hour for an event
        private double TestHour(int k, double zone, double t0, double lat)
        {
            double[] ha = new double[3];
            double a, b, c, d, e, s, z;
            double time;
            int hr, min;
            double az, dz, hz, nz;

            ha[0] = t0 - mRightAscentionArr[0] + k * mK1;
            ha[2] = t0 - mRightAscentionArr[2] + k * mK1 + mK1;

            ha[1] = (ha[2] + ha[0]) / 2;    // hour angle at half hour
            mDecensionArr[1] = (mDecensionArr[2] + mDecensionArr[0]) / 2;  // declination at half hour

            s = Math.Sin(lat * mDR);
            c = Math.Cos(lat * mDR);
            z = Math.Cos(90.833 * mDR);    // refraction + sun semidiameter at horizon

            if (k <= 0)
                mVHzArr[0] = s * Math.Sin(mDecensionArr[0]) + c * Math.Cos(mDecensionArr[0]) * Math.Cos(ha[0]) - z;

            mVHzArr[2] = s * Math.Sin(mDecensionArr[2]) + c * Math.Cos(mDecensionArr[2]) * Math.Cos(ha[2]) - z;

            if (Sign(mVHzArr[0]) == Sign(mVHzArr[2]))
                return mVHzArr[2];  // no event this hour

            mVHzArr[1] = s * Math.Sin(mDecensionArr[1]) + c * Math.Cos(mDecensionArr[1]) * Math.Cos(ha[1]) - z;

            a = 2 * mVHzArr[0] - 4 * mVHzArr[1] + 2 * mVHzArr[2];
            b = -3 * mVHzArr[0] + 4 * mVHzArr[1] - mVHzArr[2];
            d = b * b - 4 * a * mVHzArr[0];

            if (d < 0)
                return mVHzArr[2];  // no event this hour

            d = Math.Sqrt(d);
            e = (-b + d) / (2 * a);

            if ((e > 1) || (e < 0))
                e = (-b - d) / (2 * a);

            time = (double)k + e + (double)1 / (double)120; // time of an event

            hr = (int)Math.Floor(time);
            min = (int)Math.Floor((time - hr) * 60);

            hz = ha[0] + e * (ha[2] - ha[0]);                 // azimuth of the sun at the event
            nz = -Math.Cos(mDecensionArr[1]) * Math.Sin(hz);
            dz = c * Math.Sin(mDecensionArr[1]) - s * Math.Cos(mDecensionArr[1]) * Math.Cos(hz);
            az = Math.Atan2(nz, dz) / mDR;
            if (az < 0) az = az + 360;

            if ((mVHzArr[0] < 0) && (mVHzArr[2] > 0))
            {
                mRiseTimeArr[0] = hr;
                mRiseTimeArr[1] = min;
                mRizeAzimuth = az;
                mIsSunrise = true;
            }

            if ((mVHzArr[0] > 0) && (mVHzArr[2] < 0))
            {
                mSetTimeArr[0] = hr;
                mSetTimeArr[1] = min;
                mSetAzimuth = az;
                mIsSunset = true;
            }

            return mVHzArr[2];
        }

        #endregion  // Private Methods
    }

    /// <summary>
    /// Simple JSON serializer / deserializer for passing messages
    /// between processes
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// Convert a serializable object to JSON
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="data">Data model to convert to JSON</param>
        /// <returns>JSON serialized string of data model</returns>
        public static string ToJson<T>(T data)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                serializer.WriteObject(ms, data);
                var jsonArray = ms.ToArray();
                return Encoding.UTF8.GetString(jsonArray, 0, jsonArray.Length);
            }
        }

        /// <summary>
        /// Convert from JSON to a serializable object
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="json">JSON serialized object to convert from</param>
        /// <returns>Object deserialized from JSON</returns>
        public static T FromJson<T>(string json)
        {
            var deserializer = new DataContractJsonSerializer(typeof(T));
            try
            {
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                    return (T)deserializer.ReadObject(ms);
            }
            catch (SerializationException ex)
            {
                // If the string could not be deserialized to an object from JSON
                // then add the original string to the exception chain for debugging.
                throw new SerializationException("Unable to deserialize JSON: " + json, ex);
            }
        }
    }

    public static class FileHelper
    {
        public static async Task SaveFile(string sLine, string path)
        {
            StorageFolder cacheFolder = ApplicationData.Current.LocalFolder;

            StorageFile cacheFile = await cacheFolder.CreateFileAsync(path, CreationCollisionOption.ReplaceExisting);
            var flow = await cacheFile.OpenAsync(FileAccessMode.ReadWrite);
            using (var outputStream = flow.GetOutputStreamAt(0))
            {
                using (var dataWriter = new DataWriter(outputStream))
                {
                    dataWriter.WriteString(sLine);
                    await dataWriter.StoreAsync();
                    await outputStream.FlushAsync();
                }
            }
            flow.Dispose();
        }

        public static async Task<string> ReadFileasString(string path)
        {
            if (path == null)
                return null;
            StorageFolder sFolder = ApplicationData.Current.LocalFolder;
            StorageFile sFile = await sFolder.GetFileAsync(path);
            return await FileIO.ReadTextAsync(sFile);
        }

        public static async Task<IRandomAccessStream> ReadFileasStream(string path)
        {
            var stream = await LoadBitmap(path);
            stream.Seek(0);
            return stream;
        }

        public static async Task<IRandomAccessStream> LoadBitmap(string relativePath)
        {
            var s = relativePath.Substring(relativePath.LastIndexOf('/') + 1);
            var storageFile = await ApplicationData.Current.LocalFolder.GetFileAsync(s);
            try
            {
                var cache = await ApplicationData.Current.LocalCacheFolder.GetFileAsync(storageFile.Name);
                await cache.DeleteAsync();
            }
            catch (Exception)
            {

            }
            storageFile = await storageFile.CopyAsync(ApplicationData.Current.LocalCacheFolder);
            var stream = await storageFile.OpenAsync(FileAccessMode.ReadWrite);
            return stream;
        }

        public static async Task<byte[]> FetchArtwork(IStorageFile file)
        {
            if (null != file)
            {
                switch (file.FileType)
                {
                    case ".mp3": return await FetchfromMP3(file);
                    case ".m4a": return await FetchfromM4A(file);
                    case ".flac": return await FetchfromFLAC(file);
                    case ".wav": return null;
                    default:
                        return null;
                }
            }
            return null;
        }

        private static async Task<byte[]> FetchfromFLAC(IStorageFile file)
        {
            var fileStream = await file.OpenStreamForReadAsync();
            var tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name,
                             fileStream, fileStream));
            var tags = tagFile.GetTag(TagTypes.FlacMetadata);
            var p = tags.Pictures;
            if (p.Length > 0)
            {
                return p[0].Data.Data;
            }
            return null;
        }

        private static async Task<byte[]> FetchfromM4A(IStorageFile file)
        {
            var fileStream = await file.OpenStreamForReadAsync();
            var tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name,
                             fileStream, fileStream));
            var tags = tagFile.GetTag(TagTypes.Apple);
            var p = tags.Pictures;
            if (p.Length > 0)
            {
                return p[0].Data.Data;
            }
            return null;
        }

        private static async Task<byte[]> FetchfromMP3(IStorageFile file)
        {
            var fileStream = await file.OpenStreamForReadAsync();
            var tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name,
                             fileStream, fileStream));
            var tags = tagFile.GetTag(TagTypes.Id3v2);
            var p = tags.Pictures;
            if (p.Length > 0)
            {
                return p[0].Data.Data;
            }
            return null;
        }

        public static async Task<IRandomAccessStream> ToRandomAccessStream(byte[] bytestream)
        {
            InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream();
            DataWriter datawriter = new DataWriter(memoryStream.GetOutputStreamAt(0));
            datawriter.WriteBytes(bytestream);
            await datawriter.StoreAsync();
            memoryStream.Seek(0);
            return memoryStream;
        }

        public static Stream ToStream(byte[] byteStream)
        {
            Stream stream = new MemoryStream(byteStream);
            return stream;
        }
    }
}
