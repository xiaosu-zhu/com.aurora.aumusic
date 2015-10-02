using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic
{
    class SongsEnum
    {
        Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        public ObservableCollection<Song> Songs = new ObservableCollection<Song>();
        public async Task<ObservableCollection<Song>> GetSongsEnum()
        {
            List<Song> SongList = new List<Song>();
            int count = (int)localSettings.Values["FolderCount"];
            for (int i = 0; i < count; i++)
            {
                String TempPath = (String)localSettings.Values["FolderSettings" + i];
                SongList = await Song.GetSongListfromPath(TempPath);
            }
            return Songs;

        }
    }
}
