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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace com.aurora.aumusic.shared.FolderSettings
{
    public class FolderPathObservation
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        ObservableCollection<PathGroupList> Folders = new ObservableCollection<PathGroupList>();
        public List<string> PathTokens = new List<string>();

        public async Task<List<PathGroupList>> RestorePathsfromSettings()
        {
            ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["FolderSettings"];
            if (composite != null)
            {
                string TempPath;
                int count = (int)composite["FolderCount"];
                Folders.Clear();
                PathTokens.Clear();
                List<FolderItem> folders = new List<FolderItem>();
                for (int i = 0; i < count; i++)
                {
                    TempPath = (string)composite["FolderSettings" + i];
                    StorageFolder TempFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(TempPath);
                    if (TempFolder != null)
                    {
                        PathTokens.Add(TempPath);
                        folders.Add(new FolderItem(TempFolder, i));
                    }
                    else
                    {
                        continue;
                    }
                }
                return RestoreFoldertoStorage(folders);
            }
            return null;

        }

        private List<PathGroupList> RestoreFoldertoStorage(List<FolderItem> folders)
        {
            var query = from item in folders
                        group item by item.Key into g
                        orderby g.Key
                        select new { GroupName = g.Key, Items = g };
            List<PathGroupList> p = new List<PathGroupList>();
            foreach (var g in query)
            {
                PathGroupList info = new PathGroupList();
                info.Key = g.GroupName;
                foreach (var item in g.Items)
                {
                    info.Add(item);
                }
                p.Add(info);
            }
            return p;
        }

        public int i = 0;

        public void DeleteFolder(FolderItem folderItem)
        {
            foreach (var item in Folders)
            {
                if (item.Remove(folderItem))
                {
                    ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["FolderSettings"];
                    if (composite != null)
                    {
                        var TempPath = (string)composite["FolderSettings" + folderItem.i];
                        if (PathTokens.Remove(TempPath))
                        {
                            StorageApplicationPermissions.FutureAccessList.Remove(TempPath);
                            SaveFoldertoSettings();
                        }
                    }
                }
            }
        }

        public ObservableCollection<PathGroupList> GetFolders()
        {
            return Folders;
        }

        public bool SaveFoldertoStorage(StorageFolder Folder)
        {
            if (Folders.Count > 0)
            {
                foreach (var item in Folders)
                {
                    if ((char)item.Key == Folder.Path[0])
                    {
                        foreach (FolderItem folder in item)
                        {
                            if (folder.Folder.Path == Folder.Path)
                                return false;
                        }
                        ((IList)item).Add(new FolderItem(Folder));
                        Folders[Folders.IndexOf(item)] = item;
                        PathTokens.Add(StorageApplicationPermissions.FutureAccessList.Add(Folder));
                        return true;
                    }
                }
            }
            Folders.Add(new PathGroupList(Folder.Path[0], new FolderItem(Folder)));
            PathTokens.Add(StorageApplicationPermissions.FutureAccessList.Add(Folder));
            return true;
        }

        public void SaveFoldertoSettings()
        {
            ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue();
            int i = 0;
            foreach (var item in PathTokens)
            {
                composite["FolderSettings" + i.ToString()] = item;
                i++;
            }
            composite["FolderCount"] = i;
            localSettings.Values["FolderSettings"] = composite;
        }
    }

    public class PathGroupList : IList, IList<object>, INotifyCollectionChanged
    {
        public object Key;
        public List<object> PathList = new List<object>();
        public PathGroupList(char c, FolderItem folder)
        {
            this.Key = c;
            this.Add(folder);
        }

        public PathGroupList()
        {

        }

        public object this[int index]
        {
            get
            {
                if (index < PathList.Count && index >= 0)
                    return PathList[index];
                else
                    throw new IndexOutOfRangeException();
            }

            set
            {
                if (value is FolderItem)
                    if (index < PathList.Count && index >= 0)
                        PathList[index] = value;
                    else throw new IndexOutOfRangeException();
                throw new ArrayTypeMismatchException();
            }
        }

        public int Count
        {
            get
            {
                return PathList.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsFixedSize
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
                throw new NotImplementedException();
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler h = CollectionChanged;
            if (h != null)
                h(this, e);
        }

        public void Add(object item)
        {
            if (item is FolderItem)
            {
                PathList.Add(item);
                NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item);
                return;
            }
            throw new ArrayTypeMismatchException();
        }

        public void Clear()
        {
            PathList.Clear();
            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
        }

        public bool Contains(object item)
        {
            if (item is FolderItem)
                foreach (FolderItem vale in PathList)
                {
                    if (vale.Folder.Path == (item as FolderItem).Folder.Path)
                        return true;
                }
            return false;
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            if (array is FolderItem[])
            {
                if (arrayIndex < PathList.Count && arrayIndex >= 0)
                    PathList.CopyTo(array, arrayIndex);
                else throw new IndexOutOfRangeException();
            }
            else throw new ArrayTypeMismatchException();
        }

        public IEnumerator<object> GetEnumerator()
        {
            return ((IList<object>)PathList).GetEnumerator();
        }

        public int IndexOf(object item)
        {
            return PathList.IndexOf(item);
        }

        public void Insert(int index, object item)
        {
            if (item is FolderItem)
            {
                if (index < PathList.Count && index >= 0)
                    PathList.Insert(index, item);
                else throw new IndexOutOfRangeException();
            }
            else throw new ArrayTypeMismatchException();
        }

        public bool Remove(object item)
        {
            if (item is FolderItem)
            {
                NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item);
                return PathList.Remove(item);
            }
            throw new ArrayTypeMismatchException();
        }

        public void RemoveAt(int index)
        {
            if (index < PathList.Count && index >= 0)
            {
                NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, PathList[index]);
                PathList.RemoveAt(index);
                return;
            }
            else throw new IndexOutOfRangeException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IList<object>)PathList).GetEnumerator();
        }

        int IList.Add(object value)
        {
            if (value is FolderItem)
            {
                PathList.Add(value);
                NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value);
                return PathList.Count - 1;
            }
            return -1;
        }

        void IList.Remove(object value)
        {
            if (value is FolderItem)
            {
                PathList.Remove(value);
                NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value);
                return;
            }
            throw new ArrayTypeMismatchException();
        }

        public void CopyTo(Array array, int index)
        {
            PathList.CopyTo((object[])array, index);
        }
    }
}
