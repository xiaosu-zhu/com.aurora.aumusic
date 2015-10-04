using com.aurora.aumusic.Albums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace com.aurora.aumusic
{
    class SongsEnum
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public ObservableCollection<Song> Songs = new ObservableCollection<Song>();
        List<Song> SongList = new List<Song>();
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

        public ObservableCollection<Song> GetSongs(List<Song> songList)
        {
            Songs.Clear();
            if (songList != null)
            {
                foreach (var item in songList)
                {
                    Songs.Add(item);
                }
            }
            return Songs;
        }

        public async Task<List<Song>> RefreshList()
        {
            return await CreateSongListAsync();
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
    }
}
