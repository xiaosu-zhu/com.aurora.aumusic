using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared.MessageService
{
    public enum DESIREDPLAYBACKSTATE { Play, Pause, Next, Previous, Stop, Unknown };
    public enum NOWPLAYBACKSTATE { Playing, Paused, Stopped };
    public enum APPSTATE { Active, Suspended, Unknown };
    public static class MessageService
    {
        // The underlying BMP methods can pass a ValueSet. MessageService
        // relies on this to pass a type and body payload.
        const string MessageType = "MessageType";
        const string MessageBody = "MessageBody";

        public static void SendMessageToForeground<T>(T message)
        {

            //TODO
        }

        public static void SendMessageToBackground<T>(T message)
        {
            //TODO
        }

        public static DESIREDPLAYBACKSTATE GetDesiredState(ForegroundMessage Message)
        {
            return DESIREDPLAYBACKSTATE.Unknown;
        }
    }
}
