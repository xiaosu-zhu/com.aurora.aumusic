using com.aurora.aumusic.shared.Albums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;
using Windows.Media.Playback;
using Windows.Media.Core;
using com.aurora.aumusic.shared.Songs;

namespace com.aurora.aumusic.shared
{
    public class AlbumList : IList, INotifyCollectionChanged, IItemsRangeInfo,ISongModelList
    {
        private const string TrackIdKey = "trackid";
        private const string TitleKey = "title";
        private const string AlbumArtKey = "albumart";
        private List<AlbumItem> _albumList = new List<AlbumItem>();
        private ItemIndexRange LastRange;
        public object this[int index]
        {
            get
            {
                if (index < _albumList.Count)
                {
                    return _albumList[index];
                }
                else throw new IndexOutOfRangeException();
            }

            set
            {
                if (index < _albumList.Count)
                {
                    if (value is AlbumItem)
                    {
                        _albumList[index] = (AlbumItem)value;
                        return;
                    }
                    throw new ArrayTypeMismatchException();
                }
                else throw new IndexOutOfRangeException();
            }
        }

        public int Count
        {
            get
            {
                return _albumList.Count;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        public double PreScrollFactor { get; private set; }
        public double Difference { get; private set; }
        public double Weight { get; private set; }
        public ThreadPoolTimer Refresher { get; private set; }

        private ItemIndexRange LastFetch = null;

        public AlbumList()
        {
            LastRange = new ItemIndexRange(0, 0);

        }

        public int Add(object value)
        {
            if (value is AlbumItem)
                _albumList.Add((AlbumItem)value);
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, _albumList.Count - 1);
            this.OnCollectionChanged(e);
            return _albumList.Count - 1;
            throw new ArrayTypeMismatchException();
        }

        public void Clear()
        {
            _albumList.Clear();
        }

        public bool Contains(object value)
        {
            if (value is AlbumItem)
                return _albumList.Contains((AlbumItem)value);
            throw new ArrayTypeMismatchException();
        }

        public void CopyTo(Array array, int index)
        {
            int j = index;
            for (int i = 0; i < _albumList.Count; i++)
            {
                array.SetValue(_albumList[i], j);
                j++;
            }
        }

        public List<AlbumItem> ToList()
        {
            return _albumList.GetRange(0, _albumList.Count);
        }

        public IEnumerator GetEnumerator()
        {
            return _albumList.GetEnumerator();
        }

        public IEnumerable<AlbumItem> Enumerable()
        {
            return _albumList;
        }

        public int IndexOf(object value)
        {
            if (value is AlbumItem)
            {
                return _albumList.IndexOf((AlbumItem)value);
            }
            throw new ArrayTypeMismatchException();
        }

        public void Insert(int index, object value)
        {
            if (value is AlbumItem)
            {
                _albumList.Insert(index, (AlbumItem)value);
            }
            throw new ArrayTypeMismatchException();
        }

        public void Remove(object value)
        {
            if (value is AlbumItem)
            {
                NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value);
                OnCollectionChanged(e);
                _albumList.Remove((AlbumItem)value);
            }
            throw new ArrayTypeMismatchException();
        }

        public void RemoveAt(int index)
        {
                NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this[index]);
                OnCollectionChanged(e);
                _albumList.RemoveAt(index);
        }

        public void Refresh(object value)
        {
            if (value is AlbumItem)
            {
                try
                {
                    int i = _albumList.IndexOf((AlbumItem)value);
                    NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, (AlbumItem)value, _albumList[i]);
                    OnCollectionChanged(e);
                    _albumList[i] = (AlbumItem)value;
                    return;
                }
                catch (Exception)
                {
                    throw new NullReferenceException();
                }
            }
            throw new ArrayTypeMismatchException();
        }

        internal void Initial()
        {
            //if (this.Count / 100 < 2)
            //{
            this.FetchFinal(0, _albumList.Count - 1);
            //}
            //else
            //{
            //    this.FetchFinal(0, 19);
            //    LastRange = new ItemIndexRange(0, 9);
            //}
        }

        public void RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
        {

            Weight = (double)LastRange.Length / (double)LastFetch.Length;
            int _firstDelta = visibleRange.FirstIndex - this.LastRange.FirstIndex;
            int _lastDelta = visibleRange.LastIndex - this.LastRange.LastIndex;
            Task.Run(() =>
            {
                if (trackedItems != null)
                    foreach (var item in trackedItems)
                    {
                        this.FetchFinal(item.FirstIndex, item.LastIndex);
                    }
            });
            this.Difference += (_firstDelta + _lastDelta) / (double)this.LastFetch.Length;
            if (this.Difference > Weight || this.LastFetch.FirstIndex > visibleRange.FirstIndex || this.LastFetch.LastIndex < visibleRange.LastIndex)
            {
                this.Difference = 0.0;
                this.PreScrollFactor = (_firstDelta + _lastDelta) / (double)visibleRange.Length;
                int _count = (int)visibleRange.Length * 2 + 3;
                int _offset = (int)((_count - (int)visibleRange.Length) * PreScrollFactor);
                int _first = (visibleRange.FirstIndex - _count / 2 + _offset);
                int _last = (visibleRange.LastIndex + _count / 2 + _offset);
                this.FetchFinal(_first, _last);
                // NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, , _albumList.GetRange(LastFetch.FirstIndex,(int)LastFetch.Length));
                // this.OnCollectionChanged(e);
            }
            LastRange = visibleRange;
        }

        public List<SongModel> ToSongModelList()
        {
            List<SongModel> songs = new List<SongModel>();
            foreach (var album in _albumList)
            {
                foreach (var item in album.Songs)
                {
                    songs.Add(new SongModel(item));
                }
            }
            return songs;
        }

        private IList FetchFinal(int _first, int _last)
        {
            if (_first < 0)
                _first = 0;
            if (_last > _albumList.Count - 1)
                _last = _albumList.Count - 1;
            for (int i = _first; i <= _last; i++)
            {
                if (_albumList[i].IsFetched)
                {
                    continue;
                }
                _albumList[i].Fetch();
            }
            if (LastFetch != null)
            {
                var t = Task.Factory.StartNew(() =>
                           {
                               for (int i = 0; i < _first; i++)
                               {
                                   _albumList[i].IsFetched = false;
                                   _albumList[i].Collect();
                               }
                               GC.Collect();
                           });
                t.ContinueWith((task) =>
                {
                    for (int i = _albumList.Count - 1; i > _last; i--)
                    {
                        if (_albumList[i].IsFetched == false)
                            continue;
                        _albumList[i].IsFetched = false;
                        _albumList[i].Collect();
                    }
                    GC.Collect();
                });

            }
            LastFetch = new ItemIndexRange(_first, (uint)(_last - _first + 1));
            return _albumList.GetRange(_first, (_last - _first + 1));
        }

        public void AddRange(IList<AlbumItem> albums)
        {
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, albums.ToList());
            OnCollectionChanged(e);
            _albumList.AddRange(albums);
            
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler h = CollectionChanged;
            if (h != null)
                h(this, e);
        }
        private void FireCollectionReset()
        {
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(e);
        }

        public void Dispose()
        {

        }
    }
}
