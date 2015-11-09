using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using com.aurora.aumusic;

namespace com.aurora.aumusic.backgroundtask.MessageService
{
    public class ForegroundMessage
    {
        public MediaPlaybackList PlayList;
        public DESIREDPLAYBACKSTATE PlaybackState;
    }
}
