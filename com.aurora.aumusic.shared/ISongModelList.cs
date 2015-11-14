using com.aurora.aumusic.shared.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared
{
    interface ISongModelList
    {
        List<SongModel> ToSongModelList();
    }
}
