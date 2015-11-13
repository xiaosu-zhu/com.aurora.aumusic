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
    public class BackPlaybackChangedMessage
    {
        [DataMember]
        public NowPlaybackState NowState;
        [DataMember]
        public SongModel CurrentSong;

        public BackPlaybackChangedMessage(NowPlaybackState now, SongModel song)
        {
            CurrentSong = song;
            NowState = now;
        }
    }
}
