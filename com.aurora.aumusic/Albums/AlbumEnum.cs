using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI;
using Windows.UI.Xaml;

namespace com.aurora.aumusic
{
    public enum RefreshState { NeedRefresh, NeedCreate, Normal };
    public class AlbumEnum
    {
        public ObservableCollection<AlbumItem> Albums = new ObservableCollection<AlbumItem>();
        public List<AlbumItem> AlbumList = new List<AlbumItem>();
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private static readonly string[] tempTypeStrings = new[] { ".mp3", ".m4a", ".flac", ".wav" };
        private List<KeyValuePair<string, List<IStorageFile>>> RefreshList = new List<KeyValuePair<string, List<IStorageFile>>>();

        public async Task<RefreshState> getAlbumList()
        {
            RefreshState State = RefreshState.Normal;
            try
            {
                if (!(bool)localSettings.Values["isCreated"])
                {
                    return RefreshState.NeedCreate;
                }
                ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["FolderSettings"];
                int count = (int)composite["FolderCount"];
                for (int i = 0; i < count; i++)
                {
                    string tempPath = (string)composite["FolderSettings" + i.ToString()];
                    List<IStorageFile> l = await RestoreAlbumsfromStorage(tempPath);
                    if (l.Count > 0)
                    {
                        KeyValuePair<string, List<IStorageFile>> p = new KeyValuePair<string, List<IStorageFile>>(tempPath, l);
                        RefreshList.Add(p);
                        State = RefreshState.NeedRefresh;
                    }
                }
                return State;
            }
            catch (Exception)
            {
                return RefreshState.NeedCreate;
            }

        }

        internal async Task Refresh()
        {
            foreach (var item in RefreshList)
            {
                int index = Albums.Count;
                string tempPath = item.Key;
                foreach (StorageFile tempFile in item.Value)
                {
                    if (tempTypeStrings.Contains(tempFile.FileType))
                    {
                        Song song = new Song(tempFile, tempPath);
                        await song.initial();
                        await AddtoAlbum(song);
                    }
                }
                foreach (var album in Albums)
                {
                    await album.Refresh();
                }
                List<AlbumItem> afterList = Albums.ToList();
                afterList.RemoveRange(0, index);
                Task.Run(() =>
                {
                    RefreshAlbumstoStorage(afterList, tempPath);
                });
            }
        }

        public async Task FirstCreate()
        {
            if (localSettings.Values.ContainsKey("FolderSettings"))
            {
                ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["FolderSettings"];
                int Songscount = 0;
                if (composite != null)
                {
                    int count = (int)composite["FolderCount"];
                    for (int i = 0; i < count; i++)
                    {
                        String tempPath = (String)composite["FolderSettings" + i.ToString()];
                        Songscount += await GetSongs(tempPath);
                    }
                    localSettings.Values["AlbumsCount"] = Albums.Count;
                    localSettings.Values["isCreated"] = true;
                    int progress = 0;
                }
            }
        }

        private async Task<int> GetSongs(string tempPath)
        {
            StorageFolder tempFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(tempPath);
            IReadOnlyList<IStorageFile> AllList = await SearchAllinFolder(tempFolder);
            int count = AllList.Count;
            int index = Albums.Count;
            foreach (StorageFile tempFile in AllList)
            {
                if (tempTypeStrings.Contains(tempFile.FileType))
                {
                    Song song = new Song(tempFile, tempPath);
                    await song.initial();
                    await AddtoAlbum(song);
                }
            }
            foreach (var item in Albums)
            {
                await item.Refresh();
            }
            List<AlbumItem> afterList = Albums.ToList();
            afterList.RemoveRange(0, index);
            Task.Run(() =>
             {
                 saveAlbumstoStorage(afterList, tempPath);
             });
            return count;
        }

        private void RefreshAlbumstoStorage(List<AlbumItem> afterList, string tempPath)
        {
            ApplicationDataContainer MainContainer = localSettings.CreateContainer(tempPath, ApplicationDataCreateDisposition.Always);
            int i = (int)MainContainer.Values["AlbumsCount"];
            foreach (var item in afterList)
            {
                ApplicationDataContainer SubContainer = MainContainer.CreateContainer("Album" + i, ApplicationDataCreateDisposition.Always);
                int j = 0;
                foreach (var song in item.Songs)
                {
                    ApplicationDataContainer triContainer = SubContainer.CreateContainer("Songs" + j, ApplicationDataCreateDisposition.Always);
                    triContainer.Values["FolderToken"] = song.FolderToken;
                    triContainer.Values["MainKey"] = song.MainKey;
                    triContainer.Values["Title"] = song.Title;
                    triContainer.Values["ArtWork"] = song.ArtWork;
                    triContainer.Values["Album"] = song.Album;
                    triContainer.Values["Year"] = song.Year;
                    triContainer.Values["Disc"] = song.Disc;
                    triContainer.Values["DiscCount"] = song.DiscCount;
                    triContainer.Values["Track"] = song.Track;
                    triContainer.Values["TrackCount"] = song.TrackCount;
                    triContainer.Values["Rating"] = song.Rating;
                    string sb = string.Join("|:|", song.Artists);
                    triContainer.Values["Artists"] = sb;
                    sb = string.Join("|:|", song.AlbumArtists);
                    triContainer.Values["AlbumArtists"] = sb;
                    sb = string.Join("|:|", song.Genres);
                    triContainer.Values["Genres"] = sb;
                    triContainer.Values["Duration"] = song.Duration.ToString();
                    j++;
                }
                SubContainer.Values["SongsCount"] = item.Songs.Count;
                SubContainer.Values["MainColor"] = item.Palette.A + "," + item.Palette.R + "," + item.Palette.G + "," + item.Palette.B;
                SubContainer.Values["Rating"] = item.Rating;
                i++;
            }
            MainContainer.Values["AlbumsCount"] = i + 1;
        }

        private void saveAlbumstoStorage(List<AlbumItem> afterList, string tempPath)
        {
            ApplicationDataContainer MainContainer = localSettings.CreateContainer(tempPath, ApplicationDataCreateDisposition.Always);
            int i = 0;
            foreach (var item in afterList)
            {
                ApplicationDataContainer SubContainer = MainContainer.CreateContainer("Album" + i, ApplicationDataCreateDisposition.Always);
                int j = 0;
                item.Position = i;
                foreach (var song in item.Songs)
                {
                    song.Position = i;
                    song.SubPosition = j;
                    ApplicationDataContainer triContainer = SubContainer.CreateContainer("Song" + j, ApplicationDataCreateDisposition.Always);
                    triContainer.Values["FolderToken"] = song.FolderToken;
                    triContainer.Values["MainKey"] = song.MainKey;
                    triContainer.Values["Title"] = song.Title;
                    triContainer.Values["ArtWork"] = song.ArtWork;
                    triContainer.Values["Album"] = song.Album;
                    triContainer.Values["Year"] = song.Year;
                    triContainer.Values["Disc"] = song.Disc;
                    triContainer.Values["DiscCount"] = song.DiscCount;
                    triContainer.Values["Track"] = song.Track;
                    triContainer.Values["TrackCount"] = song.TrackCount;
                    triContainer.Values["Rating"] = song.Rating;
                    string sb = string.Join("|:|", song.Artists);
                    triContainer.Values["Artists"] = sb;
                    sb = string.Join("|:|", song.AlbumArtists);
                    triContainer.Values["AlbumArtists"] = sb;
                    sb = string.Join("|:|", song.Genres);
                    triContainer.Values["Genres"] = sb;
                    triContainer.Values["Duration"] = song.Duration.ToString();
                    triContainer.Values["PlayTimes"] = song.PlayTimes;
                    j++;
                }
                SubContainer.Values["SongsCount"] = item.Songs.Count;
                SubContainer.Values["MainColor"] = item.Palette.A + "," + item.Palette.R + "," + item.Palette.G + "," + item.Palette.B;
                SubContainer.Values["Rating"] = item.Rating;
                i++;
            }
            MainContainer.Values["AlbumsCount"] = afterList.Count;
        }

        private async Task AddtoAlbum(Song song)
        {
            if (Albums.Count > 0)
            {
                foreach (var item in Albums)
                {
                    if (item.AlbumName == song.Album)
                    {
                        item.Songs.Add(song);
                        item.OnPropertyChanged();
                        return;
                    }
                }
            }
            AlbumItem album = new AlbumItem();
            album.AlbumName = song.Album;
            album.Songs.Add(song);
            await album.Initial();
            Albums.Add(album);
        }

        private async Task<List<IStorageFile>> RestoreAlbumsfromStorage(string tempPath)
        {
            List<IStorageFile> AllList = new List<IStorageFile>();
            StorageFolder tempFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(tempPath);
            AllList.AddRange(await SearchAllinFolder(tempFolder));
            ApplicationDataContainer MainContainer =
           localSettings.CreateContainer(tempPath, ApplicationDataCreateDisposition.Always);
            int AlbumsCount = (int)MainContainer.Values["AlbumsCount"];
            List<Song> AllSongs = new List<Song>();
            for (int i = 0; i < AlbumsCount; i++)
            {
                List<Song> tempSongs = new List<Song>();
                ApplicationDataContainer SubContainer =
    MainContainer.CreateContainer("Album" + i, ApplicationDataCreateDisposition.Always);
                int SongsCount = (int)SubContainer.Values["SongsCount"];
                for (int j = 0; j < SongsCount; j++)
                {
                    Song tempSong = Song.RestoreSongfromStorage(AllList, SubContainer, j);
                    if (tempSong == null)
                    {
                        continue;
                    }
                    tempSong.SubPosition = j;
                    tempSongs.Add(tempSong);
                    AllList.Remove(tempSong.AudioFile);
                }
                AlbumItem tempAlbum = new AlbumItem(tempSongs);
                tempAlbum.Position = i;
                tempAlbum.Restore();
                string[] tempColor = ((string)SubContainer.Values["MainColor"]).Split(',');
                byte a = Byte.Parse(tempColor[0]), r = Byte.Parse(tempColor[1]), g = Byte.Parse(tempColor[2]), b = Byte.Parse(tempColor[3]);
                tempAlbum.Palette = Color.FromArgb(a, r, g, b);
                tempAlbum.GenerateTextColor();
                tempAlbum.Rating = (uint)SubContainer.Values["Rating"];
                await Task.Delay(4);
                Albums.Add(tempAlbum);
            }
            return AllList;
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

    }
}
