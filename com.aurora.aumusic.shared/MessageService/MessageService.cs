using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace com.aurora.aumusic.shared.MessageService
{
    public enum DESIREDPLAYBACKSTATE { Play, Pause, Next, Previous, Stop, Unknown };
    public enum NOWPLAYBACKSTATE { Playing, Paused, Stopped, Previous, Next };
    public enum APPSTATE { Active, Suspended, Unknown };
    public static class MessageService
    {
        // The underlying BMP methods can pass a ValueSet. MessageService
        // relies on this to pass a type and body payload.
        const string MessageType = "MessageType";
        const string MessageBody = "MessageBody";

        public static void SendMessageToForeground<T>(T message)
        {
            ValueSet p = new ValueSet();
            if (message is BackPlaybackChangedMessage)
            {
                p.Add("Message", message);
                Windows.Media.Playback.BackgroundMediaPlayer.SendMessageToForeground(p);
            }

        }

        public static void SendMessageToBackground<T>(T message)
        {
            ValueSet p = new ValueSet();
            if (message is ForePlaybackChangedMessage)
            {
                p.Add("Message", message);
                Windows.Media.Playback.BackgroundMediaPlayer.SendMessageToBackground(p);
            }

        }



        public static DESIREDPLAYBACKSTATE GetDesiredState(ForePlaybackChangedMessage Message)
        {
            return DESIREDPLAYBACKSTATE.Unknown;
        }
    }
}
