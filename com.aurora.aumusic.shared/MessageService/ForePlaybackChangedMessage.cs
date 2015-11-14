using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using com.aurora.aumusic;
using System.Runtime.Serialization;
using com.aurora.aumusic.shared.Songs;

namespace com.aurora.aumusic.shared.MessageService
{
    [DataContract]
    public class ForePlaybackChangedMessage
    {
        public ForePlaybackChangedMessage(PlaybackState state, List<SongModel> songs)
        {
            DesiredPlaybackState = state;
            DesiredSongs = songs;
        }

        [DataMember]
        public PlaybackState DesiredPlaybackState;
        [DataMember]
        public List<SongModel> DesiredSongs;
    }
}
