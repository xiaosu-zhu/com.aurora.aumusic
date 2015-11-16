﻿using com.aurora.aumusic.shared.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace com.aurora.aumusic.shared.MessageService
{
    [DataContract]
    public class BackPlaybackChangedMessage
    {
        [DataMember]
        public MediaPlayerState NowState;
        [DataMember]
        public SongModel CurrentSong;

        public BackPlaybackChangedMessage(MediaPlayerState now, SongModel song)
        {
            CurrentSong = song;
            NowState = now;
        }
    }
}