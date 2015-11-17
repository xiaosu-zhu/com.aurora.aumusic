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

        public ForePlaybackChangedMessage(PlaybackState state, List<SongModel> songs, SongModel song)
        {
            DesiredPlaybackState = state;
            DesiredSongs = songs;
            Index = song;
        }

        public ForePlaybackChangedMessage(PlaybackState state)
        {
            DesiredPlaybackState = state;
            DesiredSongs = null;
        }

        public ForePlaybackChangedMessage(PlaybackMode mode)
        {
            DesiredPlaybackMode = mode;
            DesiredSongs = null;
        }

        [DataMember]
        public PlaybackState DesiredPlaybackState = PlaybackState.Unknown;
        [DataMember]
        public PlaybackMode DesiredPlaybackMode = PlaybackMode.Normal;
        [DataMember]
        public List<SongModel> DesiredSongs;
        [DataMember]
        public SongModel Index = null;
    }
}
