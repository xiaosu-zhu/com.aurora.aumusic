using com.aurora.aumusic.shared.Albums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared.MessageService
{
    public class UpdatePlaybackMessage
    {
        public UpdatePlaybackMessage(List<AlbumItem> albums)
        {
            Albums = albums;
        }

        public List<AlbumItem> Albums;
    }
}
