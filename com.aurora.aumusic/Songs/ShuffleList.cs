using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace com.aurora.aumusic
{
    class ShuffleList
    {
        private static readonly int FAV_LIST_CAPACITY = 20;

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
            Random r = new Random();
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
                return first.PlayTimes.CompareTo(second.PlayTimes);
            });
            List<Song> favList = AllSongs.GetRange(0, FAV_LIST_CAPACITY);
            foreach (var item in favList)
            {
                if (item.PlayTimes == 0)
                {
                    favList.Remove(item);
                }
            }
            return favList;
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

        public void SaveFavouriteList(List<Song> favList)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer MainContainer =
    localSettings.CreateContainer("FavouriteList", ApplicationDataCreateDisposition.Always);
            int i = 0;
            foreach (var item in favList)
            {
                ApplicationDataContainer SubContainer =
                    localSettings.CreateContainer("Song" + i, ApplicationDataCreateDisposition.Always);
                SubContainer.Values["FolderToken"] = favList[i].FolderToken;
                SubContainer.Values["Position"] = favList[i].Position;
                SubContainer.Values["SubPosition"] = favList[i].SubPosition;
                try
                {
                    string key = (string)SubContainer.Values["key"];
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(key, favList[i].AudioFile);
                }
                catch (Exception)
                {
                    SubContainer.Values["Key"] = StorageApplicationPermissions.FutureAccessList.Add(favList[i].AudioFile);
                }
                i++;
            }
            MainContainer.Values["SongsCount"] = i + 1;
        }
        public async Task<List<Song>> RestoreFavouriteList()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            ApplicationDataContainer MainContainer =
    localSettings.CreateContainer("FavouriteList", ApplicationDataCreateDisposition.Always);
            int i;
            try
            {
                i = (int)MainContainer.Values["SongsCount"];
            }
            catch (Exception)
            {
                return null;
            }
            List<Song> favList = new List<Song>();
            for (int j = 0; j < i; j++)
            {
                ApplicationDataContainer SubContainer =
                     localSettings.CreateContainer("Song" + i, ApplicationDataCreateDisposition.Always);
                ApplicationDataContainer FolderContainer =
                    localSettings.CreateContainer((string)SubContainer.Values["FolderToken"], ApplicationDataCreateDisposition.Always);
                ApplicationDataContainer AlbumContainer =
                    FolderContainer.CreateContainer("Album"+(string)SubContainer.Values["Position"], ApplicationDataCreateDisposition.Always);
                ApplicationDataContainer SongContainer =
                    AlbumContainer.CreateContainer("Song" + (string)SubContainer.Values["SubPosition"], ApplicationDataCreateDisposition.Always);
                int playtimes = (int)SongContainer.Values["PlaytTimes"];
                StorageFile f = await StorageApplicationPermissions.FutureAccessList.GetFileAsync((string)SubContainer.Values["Key"]);
                Song tempSong = new Song(f);
                await tempSong.initial();
                tempSong.FolderToken = (string)SubContainer.Values["FolderToken"];
                tempSong.PlayTimes = playtimes;
                tempSong.Position = (int)SubContainer.Values["Position"];
                tempSong.SubPosition = (int)SubContainer.Values["SubPosition"];
                favList.Add(tempSong);
            }
            return favList;
        }
    }
}

