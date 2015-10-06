using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace com.aurora.aumusic
{
    public class SongsEnum : INotifyPropertyChanged
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public ObservableCollection<Song> Songs = new ObservableCollection<Song>();
        public HashSet<Song> SongList = new HashSet<Song>();
        public List<string> SongsToken = new List<string>();
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        public int Percent
        {
            get
            {
                return _percent;
            }
            set
            {
                this._percent = value;
                this.OnPropertyChanged();
            }
        }
        private int _percent;
        public SongsEnum()
        {
            Percent = 0;
        }
        private async Task<List<Song>> CreateSongListAsync()
        {
            List<Song> tempList = new List<Song>();
            ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["FolderSettings"];
            if (composite != null)
            {

                int count = (int)composite["FolderCount"];
                for (int i = 0; i < count; i++)
                {
                    String TempPath = (String)composite["FolderSettings" + i];
                    tempList.AddRange(await Song.GetSongListfromPath(TempPath));
                }
                tempList.Sort((first, second) =>
                    first.Album.CompareTo(second.Album));
                return tempList;
            }
            return null;

        }

        public void RefreshSongs(HashSet<Song> songList)
        {
            if (songList != null)
            {
                foreach (var item in songList)
                {
                    if (!Songs.Contains(item))
                    {
                        Songs.Add(item);
                    }
                }
            }
        }

        public async Task<List<Song>> RefreshList()
        {
            return await CreateSongListAsync();
        }

        public async Task GetSongsWithProgress()
        {
            Songs.Clear();
            ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["FolderSettings"];
            if (composite != null)
            {
                int count = (int)composite["FolderCount"];
                for (int i = 0; i < count; i++)
                {
                    String tempPath = (String)composite["FolderSettings" + i];
                    await Song.GetSongListWithProgress(this, tempPath);
                }
            }
            else throw new Exception();
        }

        public async Task<List<AlbumItem>> CreateAlbum()
        {
            List<AlbumItem> tempAlbumList = new List<AlbumItem>();
            if (SongList == null)
            {
                await CreateSongListAsync();
            }
            var query = from item in SongList
                        group item by item.Album into g
                        orderby g.Key
                        select new { GroupName = g.Key, Items = g };

            foreach (var g in query)
            {
                AlbumItem tempAlbum = new AlbumItem();
                tempAlbum.AlbumName = g.GroupName;
                foreach (var item in g.Items)
                {
                    tempAlbum.Songs.Add(item);
                }
                tempAlbumList.Add(tempAlbum);
            }
            return tempAlbumList;
        }

        public void SaveSongstoStorage()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            
            foreach (var item in Songs)
            {
                ApplicationDataContainer MainContainer =
    localSettings.CreateContainer(item.MainKey, ApplicationDataCreateDisposition.Always);
                item.SaveSongtoStorage(item, localSettings);
            }
            localSettings.Values["SongsCount"] = SongList.Count;

        }

        public void RestoreSongsfromStorage()
        {
            SongList.Clear();
            Songs.Clear();
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Containers.ContainsKey("SongsCacheContainer"))
            {
                ApplicationDataContainer MainContainer = localSettings.Containers["SongsCacheContainer"];
                Song.RestoreSongfromStorage(this);
            }
        }
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
