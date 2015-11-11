using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using com.aurora.aumusic;

namespace com.aurora.aumusic.shared.MessageService
{
    public class ForePlaybackChangedMessage
    {
        public DESIREDPLAYBACKSTATE PlaybackState;
    }
}
