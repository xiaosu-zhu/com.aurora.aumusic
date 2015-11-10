using com.aurora.aumusic.shared.Albums;
using com.aurora.aumusic.shared.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace com.aurora.aumusic.shared
{
    public enum PLAYBACK_STATES { SingleSong, SongList, SingleAlbum, AlbumList, Null};
    public class PlaybackPack
    {
        public MediaElement Media;
        public PlayBack PlaybackControl;
        public AlbumItem Album;
        public AlbumEnum Albums;
        public Song song;
        public List<Song> Songs;
        public PLAYBACK_STATES States;
    }
}
