using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Foundation;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using System.Diagnostics;
using Windows.Storage.AccessCache;

namespace com.aurora.aumusic
{
    public class SongsEnum : INotifyPropertyChanged
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public ObservableCollection<Song> Songs = new ObservableCollection<Song>();
        private static readonly string[] tempTypeStrings = new[] { ".mp3", ".m4a", ".flac", ".wav" };
        public List<Song> SongList = new List<Song>();
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

        public void RefreshSongs(List<Song> songList)
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


        public async Task GetSongsWithProgress()
        {
            Songs.Clear();
            if (localSettings.Values.ContainsKey("FolderSettings"))
            {
                ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["FolderSettings"];
                if (composite != null)
                {
                    int count = (int)composite["FolderCount"];
                    for (int i = 0; i < count; i++)
                    {
                        String tempPath = (String)composite["FolderSettings" + i.ToString()];
                        await Song.GetSongListWithProgress(this, tempPath);
                    }
                }
                localSettings.Values["SongsCount"] = SongList.Count;
            }

            else
            {
                throw new Exception();
            }
        }

        public async Task<List<AlbumItem>> CreateAlbums()
        {
            List<AlbumItem> tempAlbumList = new List<AlbumItem>();
            try
            {
                await GetSongsWithProgress();
            }
            catch (Exception)
            {
                throw;
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
                tempAlbum.Initial();
                tempAlbumList.Add(tempAlbum);
            }
            return tempAlbumList;
        }

        public void SaveSongstoStorage()
        {
            uint i = 0;
            foreach (var item in Songs)
            {

                item.SaveSongtoStorage(item, i);
                i++;
            }
            localSettings.Values["SongsCount"] = SongList.Count;

        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
