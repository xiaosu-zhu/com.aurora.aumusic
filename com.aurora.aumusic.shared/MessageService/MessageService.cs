using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace com.aurora.aumusic.shared.MessageService
{

    public static class MessageService
    {
        // The underlying BMP methods can pass a ValueSet. MessageService
        // relies on this to pass a type and body payload.
        public const string MessageType = "MessageType";
        public const string MessageBody = "MessageBody";

        public static void SendMessageToForeground<T>(T message)
        {
            ValueSet p = new ValueSet();
            p.Add(MessageType, typeof(T).FullName);
            p.Add(MessageBody, JsonHelper.ToJson(message));
            Windows.Media.Playback.BackgroundMediaPlayer.SendMessageToForeground(p);
        }

        public static void SendMessageToBackground<T>(T message)
        {
            ValueSet p = new ValueSet();
            p.Add(MessageType, typeof(T).FullName);
            p.Add(MessageBody, JsonHelper.ToJson(message));
            Windows.Media.Playback.BackgroundMediaPlayer.SendMessageToBackground(p);
        }

        public static bool TryParseMessage<T>(ValueSet valueSet, out T message)
        {
            object messageTypeValue;
            object messageBodyValue;

            message = default(T);

            // Get message payload
            if (valueSet.TryGetValue(MessageService.MessageType, out messageTypeValue)
                && valueSet.TryGetValue(MessageService.MessageBody, out messageBodyValue))
            {
                // Validate type
                if ((string)messageTypeValue != typeof(T).FullName)
                {
                    return false;
                }

                message = JsonHelper.FromJson<T>(messageBodyValue.ToString());
                return true;
            }

            return false;
        }

        public static DesiredPlaybackState GetDesiredState(ForePlaybackChangedMessage Message)
        {
            return DesiredPlaybackState.Unknown;
        }
    }
}
