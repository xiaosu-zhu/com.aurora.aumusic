using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace com.aurora.aumusic
{
    public class AlbumList : IList, INotifyCollectionChanged, IItemsRangeInfo
    {
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

        internal List<AlbumItem> ToList()
        {
            return _albumList.GetRange(0, _albumList.Count);
        }

        public IEnumerator GetEnumerator()
        {
            return _albumList.GetEnumerator();
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
                _albumList.Remove((AlbumItem)value);
            }
            throw new ArrayTypeMismatchException();
        }

        public void RemoveAt(int index)
        {
            _albumList.RemoveAt(index);
        }

        internal void Initial()
        {
            if (this.Count / 10 < 2)
            {
                this.FetchFinal(0, _albumList.Count - 1, LastFetch);
            }
            else
            {
                this.FetchFinal(0, 20, LastFetch);
            }
        }

        public void RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
        {
            Weight = (double)LastRange.Length / (double)visibleRange.Length;
            int _firstDelta = this.LastRange.FirstIndex - visibleRange.FirstIndex;
            int _lastDelta = this.LastRange.LastIndex - visibleRange.LastIndex;
            Task.Run(() =>
            {
                foreach (var item in trackedItems)
                {
                    this.FetchFinal(item.FirstIndex, item.LastIndex, LastFetch);
                }
            });
            this.Difference += (_firstDelta + _lastDelta) / (double)this.LastFetch.Length;
            if (this.Difference > Weight || this.LastRange.FirstIndex > visibleRange.FirstIndex || this.LastRange.LastIndex < visibleRange.LastIndex)
            {
                this.Difference = 0.0;
                this.PreScrollFactor = (_firstDelta + _lastDelta) / (double)visibleRange.Length;
                int _count = (int)visibleRange.Length * 2 + 3;
                int _offset = (int)((_count - (int)visibleRange.Length) * PreScrollFactor);
                int _first = visibleRange.FirstIndex - _count / 2 + _offset;
                int _last = visibleRange.LastIndex + _count / 2 - _offset;
                NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, this.FetchFinal(_first, _last, LastFetch), _first);
                this.OnCollectionChanged(e);
            }


        }

        private IList FetchFinal(int _first, int _last, ItemIndexRange lastFetch)
        {
            for (int i = _first; i <= _last; i++)
            {
                if (_albumList[i].IsFetched)
                {
                    continue;
                }
                _albumList[i].Fetch();
            }
            if (lastFetch != null)
            {
                var t = Task.Factory.StartNew(() =>
                           {
                               for (int i = lastFetch.FirstIndex; i < _first; i++)
                               {
                                   _albumList[i].IsFetched = false;
                                   _albumList[i].Collect();
                               }
                               GC.Collect();

                           });
                t.ContinueWith((task) =>
                {
                    for (int i = lastFetch.LastIndex; i > _last; i--)
                    {
                        _albumList[i].IsFetched = false;
                        _albumList[i].Collect();
                    }
                    GC.Collect();
                });
                t.ContinueWith((task) =>
                {
                    LastFetch = new ItemIndexRange(_first, (uint)(_last - _first + 1));
                });
            }
            return _albumList.GetRange(_first, (_last - _first + 1));
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
