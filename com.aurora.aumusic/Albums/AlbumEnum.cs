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
    public class AlbumEnum
    {
        public ObservableCollection<AlbumItem> Albums = new ObservableCollection<AlbumItem>();
        public List<AlbumItem> AlbumList = new List<AlbumItem>();
        SongsEnum AllSongs = new SongsEnum();
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private static readonly string[] tempTypeStrings = new[] { ".mp3", ".m4a", ".flac", ".wav" };

        public async Task<bool> getAlbumList()
        {
            if (localSettings.Values.ContainsKey("AlbumsCount"))
            {
                if (((int)localSettings.Values["AlbumsCount"]) != 0)
                {
                    await Task.Run(async () =>
                                    {
                                        AlbumList = await RestoreAllfromStorage();
                                    });
                    foreach (var item in AlbumList)
                    {
                        Albums.Add(item);
                    }
                    return true;
                }
            }
            return false;
        }

        public async Task<int> FirstCreate(int progress)
        {
            await Task.Run(async () =>
            {
                AlbumList = await AllSongs.CreateAlbums();
            });
            //await Task.Run(async () =>
            //{
            //    foreach (var item in AlbumList)
            //    {
            //        await item.GetPalette();
            //    }
            //});

            localSettings.Values["AlbumsCount"] = AlbumList.Count;
            return progress;
        }

        private async Task<List<AlbumItem>> RestoreAllfromStorage()
        {
            if (localSettings.Values.ContainsKey("FolderSettings"))
            {
                ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["FolderSettings"];
                if (composite != null)
                {
                    int count = (int)composite["FolderCount"];
                    List<IStorageFile> AllList = new List<IStorageFile>();
                    for (int i = 0; i < count; i++)
                    {
                        String tempPath = (String)composite["FolderSettings" + i.ToString()];
                        StorageFolder tempFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(tempPath);
                        AllList.AddRange(await SearchAllinFolder(tempFolder));
                    }
                    List<AlbumItem> tempAlbums = new List<AlbumItem>();
                    int AlbumsCount = (int)localSettings.Values["AlbumsCount"];
                    for (int i = 0; i < AlbumsCount; i++)
                    {
                        List<Song> tempSongs = new List<Song>();
                        ApplicationDataContainer MainContainer =
            localSettings.CreateContainer("Album" + i, ApplicationDataCreateDisposition.Always);
                        int SongsCount = (int)MainContainer.Values["SongsCount"];
                        for (int j = 0; j < SongsCount; j++)
                        {
                            ApplicationDataContainer SubContainer =
            MainContainer.CreateContainer("Songs" + j, ApplicationDataCreateDisposition.Always);
                            Song tempSong = Song.RestoreSongfromStorage(SongsCount, AllList, SubContainer);
                            tempSongs.Add(tempSong);
                            AllList.Remove(tempSong.AudioFile);
                        }
                        AlbumItem tempAlbum = new AlbumItem(tempSongs);
                        tempAlbum.Initial();
                        string[] tempColor = ((string)MainContainer.Values["MainColor"]).Split(',');
                        byte a = Byte.Parse(tempColor[0]), r = Byte.Parse(tempColor[1]), g = Byte.Parse(tempColor[2]), b = Byte.Parse(tempColor[3]);
                        tempAlbum.Palette = Color.FromArgb(a, r, g, b);
                        tempAlbum.Rating = (uint)MainContainer.Values["Rating"];
                        tempAlbums.Add(tempAlbum);
                    }
                    return tempAlbums;
                }
            }
            return null;

        }

        public void SaveAlltoStorage(AlbumItem Album, int progress)
        {

            ApplicationDataContainer MainContainer =
    localSettings.CreateContainer("Album" + progress, ApplicationDataCreateDisposition.Always);
            int i = 0;
            foreach (var item in Album.Songs)
            {
                ApplicationDataContainer SubContainer = MainContainer.CreateContainer("Songs" + i, ApplicationDataCreateDisposition.Always);
                SubContainer.Values["FolderToken"] = item.FolderToken;
                SubContainer.Values["MainKey"] = item.MainKey;
                SubContainer.Values["Title"] = item.Title;
                SubContainer.Values["ArtWork"] = item.ArtWork;
                SubContainer.Values["Album"] = item.Album;
                SubContainer.Values["Year"] = item.Year;
                SubContainer.Values["Disc"] = item.Disc;
                SubContainer.Values["DiscCount"] = item.DiscCount;
                SubContainer.Values["Track"] = item.Track;
                SubContainer.Values["TrackCount"] = item.TrackCount;
                SubContainer.Values["Width"] = item.ArtWorkSize.Width;
                SubContainer.Values["Height"] = item.ArtWorkSize.Height;
                SubContainer.Values["Rating"] = item.Rating;
                string sb = item.Artists != null ? string.Join("|:|", item.Artists) : null;
                SubContainer.Values["Artists"] = sb;
                sb = item.AlbumArtists != null ? string.Join("|:|", item.AlbumArtists) : null;
                SubContainer.Values["AlbumArtists"] = sb;
                sb = item.Genres != null ? string.Join("|:|", item.Genres) : null;
                SubContainer.Values["Genres"] = sb;
                SubContainer.Values["Duration"] = item.Duration.ToString();
                i++;
            }
            MainContainer.Values["SongsCount"] = Album.Songs.Count;
            MainContainer.Values["MainColor"] = Album.Palette.A + "," + Album.Palette.R + "," + Album.Palette.G + "," + Album.Palette.B;
            MainContainer.Values["Rating"] = Album.Rating;
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
