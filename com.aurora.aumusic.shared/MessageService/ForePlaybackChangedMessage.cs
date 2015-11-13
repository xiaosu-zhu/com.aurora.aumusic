using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using com.aurora.aumusic;
using System.Runtime.Serialization;

namespace com.aurora.aumusic.shared.MessageService
{
    [DataContract]
    public class ForePlaybackChangedMessage
    {
        [DataMember]
        public DesiredPlaybackState PlaybackState;
    }
}
