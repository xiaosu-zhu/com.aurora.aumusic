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
using com.aurora.aumusic.shared.Albums;
using com.aurora.aumusic.shared.Songs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace com.aurora.aumusic.shared
{
    public class PlayBack
    {
        private List<Song> Songs = new List<Song>();
        private int NowIndex = -1;

        public event NotifyPlayBackEventHandler NotifyPlayBackEvent;
        public delegate void NotifyPlayBackEventHandler(object sender, NotifyPlayBackEventArgs e);

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
            foreach (AlbumItem item in Albums.albumList)
            {
                Songs.AddRange(item.Songs);
            }
        }

        public PlayBack()
        {
        }
        #endregion
        #region
        /// <summary>
        /// 
        /// </summary>
        /// <param name="S"></param>
        public void addNew(List<Song> S)
        {
            this.Songs.AddRange(S);
        }
        public void addNew(AlbumItem Album)
        {
            this.Songs.AddRange(Album.Songs);
        }
        public void addNew(AlbumEnum Albums)
        {
            foreach (AlbumItem item in Albums.albumList)
            {
                Songs.AddRange(item.Songs);
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
            if (Songs.Count != 0)
            {
                if (Songs.Contains(a))
                {
                    NowIndex = Songs.IndexOf(a);
                }
                else
                {
                    Songs.Add(a);
                    NowIndex = Songs.IndexOf(a);
                }
            }
            else
            {
                Songs.Add(a);
                NowIndex = 0;
            }
            await Task.Run(() =>
            {
                a.PlayOnce();
            });

            var stream = await a.AudioFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
            m.SetSource(stream, a.AudioFile.ContentType);
            OnNotifyPlayBackEvent(a);
        }
        public async Task Play(int index, MediaElement m)
        {
            if (Songs.Count >= index)
            {
                NowIndex = index;
                await Task.Run(() =>
                {
                    Songs[index].PlayOnce();
                });
                var stream = await Songs[index].AudioFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                m.SetSource(stream, Songs[index].AudioFile.ContentType);
                OnNotifyPlayBackEvent(Songs[index]);
            }
            else
            {
                throw new ArgumentNullException();
            }
        }
        public async Task Play(MediaElement m)
        {
            if (Songs.Count != 0)
            {
                NowIndex = 0;
                var stream = await Songs[0].AudioFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                m.SetSource(stream, Songs[0].AudioFile.ContentType);
                OnNotifyPlayBackEvent(Songs[0]);
            }
            else
            {
                throw new ArgumentNullException();
            }
        }
        public async Task Play(List<Song> Songs, MediaElement m)
        {
            this.Songs.Clear();
            this.Songs.AddRange(Songs);
            NowIndex = 0;
            var stream = await Songs[NowIndex].AudioFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
            m.SetSource(stream, Songs[NowIndex].AudioFile.ContentType);
            OnNotifyPlayBackEvent(Songs[NowIndex]);
            await Task.Run(() =>
            {
                foreach (var item in Songs)
                {
                    item.PlayOnce();
                }
            });

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
            OnNotifyPlayBackEvent(Songs[NowIndex]);
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
            OnNotifyPlayBackEvent(Songs[NowIndex]);
        }

        public Song NowPlaying()
        {
            if (Songs.Count != 0)
            {
                return Songs[NowIndex];
            }
            else
            {
                return null;
            }
           
        }
        public void OnNotifyPlayBackEvent(Song song)
        {
            NotifyPlayBackEventHandler h = NotifyPlayBackEvent;
            if (h != null)
                h(this, new NotifyPlayBackEventArgs(song));
        }
    }

    public class NotifyPlayBackEventArgs
    {
        public NotifyPlayBackEventArgs(AlbumItem playingalbum, Song playingsong)
        {
            this.PlayingAlbum = playingalbum;
            this.PlayingSong = playingsong;
        }

        public NotifyPlayBackEventArgs(Song playingsong)
        {
            this.PlayingSong = playingsong;
        }

        public AlbumItem PlayingAlbum;
        public Song PlayingSong;
    }
}
