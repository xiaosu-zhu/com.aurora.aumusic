using com.aurora.aumusic.shared.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared.MessageService
{
    [DataContract]
    public class NowListMessage
    {
        [DataMember]
        public List<SongModel> CurrentSongs;

        public NowListMessage(List<SongModel> songs)
        {
            CurrentSongs = songs;
        }
    }
}
