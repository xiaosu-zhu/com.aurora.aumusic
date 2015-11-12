using com.aurora.aumusic.shared.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared.MessageService
{
    public class BackPlaybackChangedMessage
    {
        public NowPlaybackState NowState;
        public Song CurrentSong;

        public BackPlaybackChangedMessage(NowPlaybackState now, Song song)
        {
            CurrentSong = song;
            NowState = now;
        }
    }
}
