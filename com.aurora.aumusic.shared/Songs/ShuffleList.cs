using com.aurora.aumusic.shared.Albums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace com.aurora.aumusic.shared.Songs
{
    public class ShuffleList
    {
        public const int FAV_LIST_CAPACITY = 20;

        List<Song> AllSongs = new List<Song>();
        public ShuffleList(List<AlbumItem> Albums)
        {
            foreach (var item in Albums)
            {
                AllSongs.AddRange(item.Songs);
            }
        }
        public List<Song> GenerateNewList(int count)
        {
            Random r = new Random(Guid.NewGuid().GetHashCode());
            List<Song> shuffleList = new List<Song>();
            for (int i = 0; i < count; i++)
            {
                shuffleList.Add(AllSongs[r.Next(AllSongs.Count)]);
            }
            return shuffleList;

        }
        public List<Song> GenerateFavouriteList()
        {
            AllSongs.Sort((first, second) =>
            {
                return -first.PlayTimes.CompareTo(second.PlayTimes);
            });
            List<Song> favList = new List<Song>();
            if (AllSongs.Count > FAV_LIST_CAPACITY)
                favList.AddRange(AllSongs.GetRange(0, FAV_LIST_CAPACITY));
            else
                favList.AddRange(AllSongs);
            List<Song> list = new List<Song>();
            foreach (var item in favList)
            {
                if (item.PlayTimes == 0)
                {
                    continue;
                }
                list.Add(item);
            }
            return list;
        }

        internal static void Save(Song song)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer MainContainer =
    localSettings.CreateContainer(song.FolderToken, ApplicationDataCreateDisposition.Always);
            ApplicationDataContainer SubContainer =
    MainContainer.CreateContainer("Album" + song.Position, ApplicationDataCreateDisposition.Always);
            ApplicationDataContainer triContainer =
    SubContainer.CreateContainer("Song" + song.SubPosition, ApplicationDataCreateDisposition.Always);
            triContainer.Values["PlayTimes"] = song.PlayTimes;
        }

        public static void SaveFavouriteList(List<Song> favList)
        {
            if (favList == null || favList.Count == 0)
                return;
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer MainContainer =
    localSettings.CreateContainer("FavouriteList", ApplicationDataCreateDisposition.Always);
            int i = 0;
            foreach (var item in favList)
            {
                ApplicationDataContainer SubContainer =
                    MainContainer.CreateContainer("FavSong" + i, ApplicationDataCreateDisposition.Always);
                SubContainer.Values["FolderToken"] = item.FolderToken;
                SubContainer.Values["Position"] = item.Position;
                SubContainer.Values["SubPosition"] = item.SubPosition;
                SubContainer.Values["Key"] = StorageApplicationPermissions.FutureAccessList.Add(item.AudioFile);
                i++;
            }
            MainContainer.Values["FavSongsCount"] = i;
        }
        public static async Task<List<Song>> RestoreFavouriteList()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer MainContainer =
    localSettings.CreateContainer("FavouriteList", ApplicationDataCreateDisposition.Always);
            int i;
            try
            {
                i = (int)MainContainer.Values["FavSongsCount"];
                List<Song> favList = new List<Song>();
                for (int j = 0; j < i; j++)
                {
                    ApplicationDataContainer SubContainer =
                         MainContainer.CreateContainer("FavSong" + j, ApplicationDataCreateDisposition.Always);
                    ApplicationDataContainer FolderContainer =
                        localSettings.CreateContainer((string)SubContainer.Values["FolderToken"], ApplicationDataCreateDisposition.Always);
                    ApplicationDataContainer AlbumContainer =
                        FolderContainer.CreateContainer("Album" + (int)SubContainer.Values["Position"], ApplicationDataCreateDisposition.Always);
                    string key = (string)SubContainer.Values["key"];
                    StorageFile file;
                    try
                    {
                        file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(key);
                        StorageApplicationPermissions.FutureAccessList.Remove(key);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    Song tempSong = Song.RestoreSongfromStorage(AlbumContainer, (int)SubContainer.Values["SubPosition"]);
                    tempSong.AudioFile = file;
                    tempSong.Position = (int)SubContainer.Values["Position"];
                    tempSong.SubPosition = (int)SubContainer.Values["SubPosition"];
                    favList.Add(tempSong);
                }
                return favList;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void SaveShuffleList(List<Song> songs)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer MainContainer =
    localSettings.CreateContainer("ShuffleList", ApplicationDataCreateDisposition.Always);
            int i = 0;
            foreach (var item in songs)
            {
                ApplicationDataContainer SubContainer =
                    MainContainer.CreateContainer("ShuSong" + i, ApplicationDataCreateDisposition.Always);
                SubContainer.Values["FolderToken"] = item.FolderToken;
                SubContainer.Values["Position"] = item.Position;
                SubContainer.Values["SubPosition"] = item.SubPosition;
                SubContainer.Values["Key"] = StorageApplicationPermissions.FutureAccessList.Add(item.AudioFile);
                i++;
            }
            MainContainer.Values["ShuSongsCount"] = i;
        }

        public static async Task<List<Song>> RestoreShuffleList()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            try
            {
                ApplicationDataContainer MainContainer =
                   localSettings.CreateContainer("ShuffleList", ApplicationDataCreateDisposition.Always);
                int i = (int)MainContainer.Values["ShuSongsCount"];
                List<Song> songs = new List<Song>();
                for (int j = 0; j < i; j++)
                {
                    ApplicationDataContainer SubContainer =
                        MainContainer.CreateContainer("ShuSong" + j, ApplicationDataCreateDisposition.Always);
                    ApplicationDataContainer FolderContainer =
                       localSettings.CreateContainer((string)SubContainer.Values["FolderToken"], ApplicationDataCreateDisposition.Always);
                    ApplicationDataContainer AlbumContainer =
                        FolderContainer.CreateContainer("Album" + (int)SubContainer.Values["Position"], ApplicationDataCreateDisposition.Always);
                    string key = (string)SubContainer.Values["key"];
                    StorageFile file;
                    try
                    {
                        file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(key);
                        StorageApplicationPermissions.FutureAccessList.Remove(key);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    Song tempSong = Song.RestoreSongfromStorage(AlbumContainer, (int)SubContainer.Values["SubPosition"]);
                    tempSong.AudioFile = file;
                    tempSong.Position = (int)SubContainer.Values["Position"];
                    tempSong.SubPosition = (int)SubContainer.Values["SubPosition"];
                    songs.Add(tempSong);
                }
                return songs;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public static List<Song> ShuffleIt(List<Song> shuffleList)
        {
            if (shuffleList == null || shuffleList.Count == 0)
                return null;
            if (shuffleList.Count <= FAV_LIST_CAPACITY)
                return shuffleList;
            Random r = new Random(Guid.NewGuid().GetHashCode());
            List<Song> ts = new List<Song>();
            for(int i = 0; i < FAV_LIST_CAPACITY; i++)
            {
                var s = r.Next(shuffleList.Count);
                ts.Add(shuffleList[s]);
                shuffleList.RemoveAt(s);
            }
            return ts;
        }
    }
}

