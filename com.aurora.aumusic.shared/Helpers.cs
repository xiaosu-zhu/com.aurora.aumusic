using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using TagLib.Mpeg;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

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

    }
    public static class EnumHelper
    {
        public static T Parse<T>(string value) where T : struct
        {
            return (T)Enum.Parse(typeof(T), value);
        }
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

        public static async Task<IRandomAccessStream> ToStream(byte[] bytestream)
        {
            InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream();
            DataWriter datawriter = new DataWriter(memoryStream.GetOutputStreamAt(0));
            datawriter.WriteBytes(bytestream);
            await datawriter.StoreAsync();
            memoryStream.Seek(0);
            return memoryStream;
        }
    }
}
