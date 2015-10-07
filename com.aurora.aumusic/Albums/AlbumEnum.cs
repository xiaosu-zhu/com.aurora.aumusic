using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic
{
    public class AlbumEnum
    {
        public ObservableCollection<AlbumItem> Albums = new ObservableCollection<AlbumItem>();
        public List<AlbumItem> AlbumList = new List<AlbumItem>();
        SongsEnum AllSongs = new SongsEnum();

        public async Task getAlbumList()
        {
            AlbumList = await AllSongs.CreateAlbums();
            foreach (var item in AlbumList)
            {
                Albums.Add(item);
            }
        }
    }
}
