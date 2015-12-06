using System.Collections.Generic;
using Windows.Storage;

namespace com.aurora.aumusic.shared.Albums
{
    public class NotifyRefreshEventArgs
    {
        public KeyValuePair<string, List<IStorageFile>> item;

        public NotifyRefreshEventArgs(KeyValuePair<string, List<IStorageFile>> item)
        {
            this.item = item;
        }
    }
}