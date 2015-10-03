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
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public ObservableCollection<Song> Songs = new ObservableCollection<Song>();
        public async Task<ObservableCollection<Song>> GetSongsEnum()
        {
            ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["FolderSettings"];
            if (composite != null)
            {
                List<Song> SongList = new List<Song>();
                int count = (int)composite["FolderCount"];
                for (int i = 0; i < count; i++)
                {
                    String TempPath = (String)composite["FolderSettings" + i];
                    SongList = await Song.GetSongListfromPath(TempPath);
                }
                return Songs;
            }
            return null;

        }
    }
}
