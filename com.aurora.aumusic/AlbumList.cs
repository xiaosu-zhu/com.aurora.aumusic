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
    class AlbumList : IList, INotifyCollectionChanged, IItemsRangeInfo
    {
        private List<AlbumItem> _albumList;
        private int _count;
        public object this[int index]
        {
            get
            {
                if (index < _count)
                {
                    return _albumList[index];
                }
                else throw new IndexOutOfRangeException();
            }

            set
            {
                if (index < _count)
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

        public int Add(object value)
        {
            if (value is AlbumItem)
                _albumList.Add((AlbumItem)value);
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
            for(int i =0;i<_albumList.Count;i++)
            {
                array.SetValue(_albumList[i], j);
                j++;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return _albumList.GetEnumerator();
        }

        public int IndexOf(object value)
        {
            if(value is AlbumItem)
            {
                return _albumList.IndexOf((AlbumItem)value);
            }
            throw new ArrayTypeMismatchException();
        }

        public void Insert(int index, object value)
        {
            if(value is AlbumItem)
            {
                _albumList.Insert(index, (AlbumItem)value);
            }
            throw new ArrayTypeMismatchException();
        }

        public void Remove(object value)
        {
            if(value is AlbumItem)
            {
                _albumList.Remove((AlbumItem)value);
            }
            throw new ArrayTypeMismatchException();
        }

        public void RemoveAt(int index)
        {
            _albumList.RemoveAt(index);
        }

        public void RangesChanged(ItemIndexRange visibleRange, IReadOnlyList<ItemIndexRange> trackedItems)
        {
            throw new NotImplementedException();
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

        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~AlbumList() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
