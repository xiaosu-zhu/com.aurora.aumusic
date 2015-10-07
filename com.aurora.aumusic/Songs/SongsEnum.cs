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

        public async Task RestoreSongsWithProgress()
        {
            if (localSettings.Values.ContainsKey("SongsCount"))
            {
                if (localSettings.Values.ContainsKey("FolderSettings"))
                {
                    ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["FolderSettings"];
                    if (composite != null)
                    {
                        int count = (int)composite["FolderCount"];
                        int SongsCount = (int)localSettings.Values["SongsCount"];
                        List<IStorageFile> AllList = new List<IStorageFile>();
                        await Task.Run(async () =>
                        {
                            int step = 0;
                            for (int i = 0; i < count; i++)
                            {
                                String tempPath = (String)composite["FolderSettings" + i];
                                StorageFolder tempFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(tempPath);
                                AllList.AddRange(await SearchAllinFolder(tempFolder));
                            }
                            for (int progress = 0; progress < SongsCount; progress++)
                            {
                                Song a = Song.RestoreSongfromStorage(progress, AllList);
                                SongList.Add(a);
                                AllList.Remove(a.AudioFile);
                                if (((double)(progress - step)) / ((double)SongsCount) > 0.01)
                                {
                                    RefreshSongs(SongList);
                                    step = progress;
                                    Percent = (int)(((double)step / SongsCount) * 100);
                                }
                            }

                        }
                        );
                    }
                }

                else
                {
                    await GetSongsWithProgress();
                }
            }
        }
        private async Task<List<IStorageFile>> SearchAllinFolder(StorageFolder tempFolder)
        {
            IReadOnlyList<IStorageItem> tempList = await tempFolder.GetItemsAsync();
            List<IStorageFile> finalList = new List<IStorageFile>();
            foreach (var item in tempList)
            {
                if (item is StorageFolder)
                {
                    finalList.AddRange(await SearchAllinFolder((StorageFolder)item));
                }
                if (item is StorageFile)
                {
                    if (tempTypeStrings.Contains(((StorageFile)item).FileType))
                    {
                        finalList.Add((StorageFile)item);
                    }
                }
            }
            return finalList;
        }

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

        public async Task<List<Song>> RefreshList()
        {
            return await CreateSongListAsync();
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
                        String tempPath = (String)composite["FolderSettings" + i];
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
            if (SongList.Count == 0)
            {
                await RestoreSongsWithProgress();
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
