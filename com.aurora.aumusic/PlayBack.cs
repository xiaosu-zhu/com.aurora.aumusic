using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace com.aurora.aumusic
{
    public class PlayBack
    {
        private List<Song> Songs = null;
        private int NowIndex = -1;
        #region
        public PlayBack(List<Song> Songs)
        {
            this.Songs = Songs;
        }
        public PlayBack(AlbumItem Album)
        {
            this.Songs = Album.Songs;
        }
        public PlayBack(AlbumEnum Albums)
        {
            this.Songs = new List<Song>();
            foreach (var item in Albums.AlbumList)
            {
                Songs.AddRange(item.Songs);
            }
        }
        #endregion
        #region
        /// <summary>
        /// 
        /// </summary>
        /// <param name="S"></param>
        public void addNew(List<Song> S)
        {
            if (this.Songs != null)
            {
                this.Songs.AddRange(S);
            }
            else
            {
                this.Songs = S;
            }
        }
        public void addNew(AlbumItem Album)
        {
            if (this.Songs != null)
            {
                this.Songs.AddRange(Album.Songs);
            }
            else
            {
                this.Songs = Album.Songs;
            }
        }
        public void addNew(AlbumEnum Albums)
        {
            if (this.Songs != null)
            {
                foreach (var item in Albums.AlbumList)
                {
                    Songs.AddRange(item.Songs);
                }
            }
            else
            {
                Songs = new List<Song>();
                foreach (var item in Albums.AlbumList)
                {
                    Songs.AddRange(item.Songs);
                }
            }
        }
        #endregion
        public void Clear()
        {
            if (this.Songs != null)
            {
                this.Songs.Clear();
            }
        }
        #region
        public async Task Play(Song a, MediaElement m)
        {
            if (Songs != null && Songs.Count != 0)
            {
                if (!Songs.Contains(a))
                {
                    Songs.Add(a);
                    NowIndex = 0;
                }
            }
            else
            {
                Songs = new List<Song>();
                Songs.Add(a);
                NowIndex = Songs.IndexOf(a);
            }
            var stream = await a.AudioFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
            m.SetSource(stream, a.AudioFile.ContentType);
        }
        public async Task Play(int index, MediaElement m)
        {
            if (Songs != null && Songs.Count >= index)
            {
                NowIndex = index;
                var stream = await Songs[index].AudioFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                m.SetSource(stream, Songs[index].AudioFile.ContentType);
            }
            else
            {
                throw new ArgumentNullException();
            }
        }
        public async Task Play(MediaElement m)
        {
            if (Songs != null && Songs.Count != 0)
            {
                NowIndex = 0;
                var stream = await Songs[0].AudioFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                m.SetSource(stream, Songs[0].AudioFile.ContentType);
            }
            else
            {
                throw new ArgumentNullException();
            }
        }
        #endregion
        public async Task PlayNext(MediaElement m)
        {
            if (NowIndex != -1 && NowIndex < Songs.Count - 1)
            {
                NowIndex++;
            }
            else
            {
                NowIndex = 0;
            }
            var stream = await Songs[NowIndex].AudioFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
            m.SetSource(stream, Songs[NowIndex].AudioFile.ContentType);
        }
        public async Task PlayPrevious(MediaElement m)
        {
            if (NowIndex > 0)
            {
                NowIndex--;
            }
            else
            {
                NowIndex = Songs.Count - 1;
            }
            var stream = await Songs[NowIndex].AudioFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
            m.SetSource(stream, Songs[NowIndex].AudioFile.ContentType);
        }

    }
}
