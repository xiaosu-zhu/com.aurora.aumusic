//Copyright(C) 2015 Aurora Studio

//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
//to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
//WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.



/// <summary>
/// Usings
/// </summary>
using System.Collections.Generic;
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
