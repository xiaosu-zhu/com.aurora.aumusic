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
using com.aurora.aumusic.shared.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI;


namespace com.aurora.aumusic.shared.Albums
{
    public enum RefreshState { NeedRefresh, NeedCreate, Normal };
    public class AlbumEnum
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private static readonly string[] tempTypeStrings = new[] { ".mp3", ".m4a", ".flac", ".wav" };
        public AlbumList albumList = new AlbumList();
        public List<AlbumItem> albums = new List<AlbumItem>();
        private List<KeyValuePair<string, List<IStorageFile>>> refreshList;
        public List<IStorageFile> AllList = new List<IStorageFile>();
        public event AlbumCreateProgressChangeHandler progresschanged = delegate { };
        public delegate void AlbumCreateProgressChangeHandler(object sender, AlbumProgressChangedEventArgs e);
        public event NotifyRefreshHandler notifyrefresh = delegate { };
        public delegate void NotifyRefreshHandler(object sender, NotifyRefreshEventArgs e);

        public RefreshState RestoreAlbums()
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
                    try
                    {
                        RestoreAlbumsfromStorage(tempPath);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                return State;
            }
            catch (Exception)
            {
                return RefreshState.NeedCreate;
            }

        }

        public async Task Refresh()
        {
            refreshList = new List<KeyValuePair<string, List<IStorageFile>>>();
            ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["FolderSettings"];
            int count = (int)composite["FolderCount"];
            for (int i = 0; i < count; i++)
            {
                List<IStorageFile> files = new List<IStorageFile>();
                string tempPath = (string)composite["FolderSettings" + i.ToString()];
                try
                {
                    StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(tempPath);
                    files.AddRange(await AlbumEnum.SearchAllinFolder(folder));
                    refreshList.Add(new KeyValuePair<string, List<IStorageFile>>(tempPath, files));
                }
                catch (Exception)
                {
                    continue;
                }

            }
            for (int k = refreshList.Count - 1; k >= 0; k--)
            {
                for (int j = refreshList[k].Value.Count - 1; j >= 0; j--)
                {
                    foreach (var key in albumList.ToSongModelList())
                    {
                        if (key.MainKey == (((StorageFile)refreshList[k].Value[j]).Path))
                        {

                            refreshList[k].Value.RemoveAt(j);
                            break;
                        }
                    }
                }
                if (refreshList[k].Value.Count == 0)
                    refreshList.RemoveAt(k);
            }
            foreach (var item in refreshList)
            {
                this.OnNotifyRefresh(item);
            }
        }

        private void OnNotifyRefresh(KeyValuePair<string, List<IStorageFile>> item)
        {
            NotifyRefreshEventArgs e = new NotifyRefreshEventArgs(item);
            this.notifyrefresh(this, e);
        }

        public async Task RefreshtoList(KeyValuePair<string, List<IStorageFile>> item)
        {
            int index = albums.Count;
            string tempPath = item.Key;
            foreach (IStorageFile tempFile in item.Value)
            {
                if (tempTypeStrings.Contains(tempFile.FileType))
                {
                    Song song = new Song((StorageFile)tempFile, tempPath);
                    await song.initial();
                    await AddtoAlbum_first(song);
                }
            }
            foreach (AlbumItem album in albumList)
            {
                await album.Refresh();
            }
            List<AlbumItem> afterList = albums.ToList();
            afterList.RemoveRange(0, index);
            albums.AddRange(afterList);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            Task.Run(() =>
            {
                RefreshAlbumstoStorage(afterList, tempPath);
            });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
        }

        public void CopytoAlbumList()
        {
            if (albums.Count > 0)
            {
                foreach (var item in albums)
                {
                    albumList.Add(item);
                }
            }
        }

        public async Task FirstCreate()
        {
            if (localSettings.Values.ContainsKey("FolderSettings"))
            {
                albumList.Clear();
                ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["FolderSettings"];
                int Songscount = 0;
                if (composite != null)
                {

                    int count = (int)composite["FolderCount"];
                    for (int i = 0; i < count; i++)
                    {
                        String tempPath = (String)composite["FolderSettings" + i.ToString()];
                        Songscount += await GetSongs(tempPath, (double)i / (double)count, count);
                    }
                    localSettings.Values["AlbumsCount"] = albums.Count;
                    localSettings.Values["isCreated"] = true;
                }
            }
            albums.AddRange(albumList.Enumerable());
        }

        private async Task<int> GetSongs(string tempPath, double percent, int total)
        {
            StorageFolder tempFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(tempPath);
            IReadOnlyList<IStorageFile> AllList = await SearchAllinFolder(tempFolder);
            int count = AllList.Count;
            int index = albumList.Count;
            int progress = 0;
            if (count == 0)
            {
                OnProgressChanged(1 / total, percent);
                return 0;
            }
            foreach (StorageFile tempFile in AllList)
            {
                if (tempTypeStrings.Contains(tempFile.FileType))
                {
                    Song song = new Song(tempFile, tempPath);
                    await song.initial();
                    await AddtoAlbum_first(song);
                }
                progress++;
                OnProgressChanged((double)progress / ((double)count * total), percent);
            }
            foreach (AlbumItem item in albumList)
            {
                await item.Refresh();
            }
            List<AlbumItem> afterList = albumList.ToList();
            afterList.RemoveRange(0, index);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            Task.Run(() =>
             {
                 saveAlbumstoStorage(afterList, tempPath);
             });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
            return count;
        }

        private void OnProgressChanged(double current, double total)
        {
            AlbumProgressChangedEventArgs e = new AlbumProgressChangedEventArgs(current, total);
            this.progresschanged(this, e);
        }

        private async Task AddtoAlbum_first(Song song)
        {
            if (albumList.Count > 0)
            {
                foreach (AlbumItem item in albumList)
                {
                    if (item.AlbumName == song.Album)
                    {
                        foreach (var s in item.Songs)
                        {
                            if (song.Title == s.Title && song.ArtWork == s.ArtWork)
                            {
                                if (PasswordEquals(song.AlbumArtists, s.AlbumArtists) && PasswordEquals(song.Artists, s.Artists))
                                    return;
                            }

                        }
                        item.Songs.Add(song);
                        return;
                    }
                }
            }
            AlbumItem album = new AlbumItem();
            album.AlbumName = song.Album;
            album.Songs.Add(song);
            await album.Initial();
            albumList.Add(album);
        }

        private void RefreshAlbumstoStorage(List<AlbumItem> afterList, string tempPath)
        {
            if (afterList.Count == 0)
                return;
            ApplicationDataContainer MainContainer = localSettings.CreateContainer(tempPath, ApplicationDataCreateDisposition.Always);
            int i = (int)MainContainer.Values["AlbumsCount"];
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
                    triContainer.Values["Duration"] = song.Duration;
                    triContainer.Values["PlayTimes"] = song.PlayTimes;
                    triContainer.Values["Width"] = song.ArtWorkSize.Width;
                    triContainer.Values["Height"] = song.ArtWorkSize.Height;
                    j++;
                }
                SubContainer.Values["SongsCount"] = item.Songs.Count;
                SubContainer.Values["MainColor"] = item.Palette.A + "," + item.Palette.R + "," + item.Palette.G + "," + item.Palette.B;
                SubContainer.Values["Rating"] = item.Rating;
                SubContainer.Values["ArtWork"] = item.AlbumArtWork;
                SubContainer.Values["ArtWorkSize"] = item.ArtWorkSize;
                SubContainer.Values["AlbumName"] = item.AlbumName;
                i++;
            }
            MainContainer.Values["AlbumsCount"] = i;
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
                    triContainer.Values["Duration"] = song.Duration;
                    triContainer.Values["PlayTimes"] = song.PlayTimes;
                    triContainer.Values["Width"] = song.ArtWorkSize.Width;
                    triContainer.Values["Height"] = song.ArtWorkSize.Height;
                    triContainer.Values["Loved"] = song.Loved;
                    j++;
                }
                SubContainer.Values["SongsCount"] = item.Songs.Count;
                SubContainer.Values["MainColor"] = item.Palette.A + "," + item.Palette.R + "," + item.Palette.G + "," + item.Palette.B;
                SubContainer.Values["Rating"] = item.Rating;
                SubContainer.Values["AlbumArtWork"] = item.AlbumArtWork;
                SubContainer.Values["AlbumName"] = item.AlbumName;
                SubContainer.Values["ArtWorkSize"] = item.ArtWorkSize;
                i++;
            }
            MainContainer.Values["AlbumsCount"] = afterList.Count;
        }

        private async Task AddtoAlbum(Song song)
        {
            if (albums.Count > 0)
            {
                foreach (AlbumItem item in albums)
                {
                    if (item.AlbumName == song.Album)
                    {
                        foreach (var s in item.Songs)
                        {
                            if (song.Title == s.Title && song.ArtWork == s.ArtWork)
                            {
                                if (PasswordEquals(song.AlbumArtists, s.AlbumArtists) && PasswordEquals(song.Artists, s.Artists))
                                    return;
                            }

                        }
                        item.Songs.Add(song);
                        return;
                    }
                }
            }
            AlbumItem album = new AlbumItem();
            album.AlbumName = song.Album;
            album.Songs.Add(song);
            await album.Initial();
            albums.Add(album);
        }

        private bool PasswordEquals(string[] b1, string[] b2)
        {
            if (b1.Length != b2.Length) return false;
            if (b1 == null || b2 == null) return false;
            for (int i = 0; i < b1.Length; i++)
                if (b1[i] != b2[i])
                    return false;
            return true;
        }
        private void RestoreAlbumsfromStorage(string tempPath)
        {
            ApplicationDataContainer MainContainer =
           localSettings.CreateContainer(tempPath, ApplicationDataCreateDisposition.Always);
            int AlbumsCount = (int)MainContainer.Values["AlbumsCount"];
            for (int i = 0; i < AlbumsCount; i++)
            {
                ApplicationDataContainer SubContainer =
    MainContainer.CreateContainer("Album" + i, ApplicationDataCreateDisposition.Always);
                AlbumItem tempAlbum = new AlbumItem();
                tempAlbum.AlbumName = (string)SubContainer.Values["AlbumName"];
                tempAlbum.ArtWorkSize = (Size)SubContainer.Values["ArtWorkSize"];
                tempAlbum.Position = i;
                string[] tempColor = ((string)SubContainer.Values["MainColor"]).Split(',');
                byte a = Byte.Parse(tempColor[0]), r = Byte.Parse(tempColor[1]), g = Byte.Parse(tempColor[2]), b = Byte.Parse(tempColor[3]);
                tempAlbum.Palette = Color.FromArgb(a, r, g, b);
                tempAlbum.GenerateTextColor();
                tempAlbum.Rating = (uint)SubContainer.Values["Rating"];
                tempAlbum.FolderToken = tempPath;
                tempAlbum.Fetch();
                albums.Add(tempAlbum);
            }
        }



        public static async Task<List<IStorageFile>> SearchAllinFolder(StorageFolder tempFolder)
        {
            try
            {
                IReadOnlyList<IStorageItem> tempList = await tempFolder.GetItemsAsync();
                List<IStorageFile> finalList = new List<IStorageFile>();
                if (tempList.Count == 0)
                    return finalList;
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
            catch (Exception)
            {
                return new List<IStorageFile>();
            }

        }

    }

}
