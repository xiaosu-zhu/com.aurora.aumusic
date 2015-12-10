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
using Windows.Foundation.Collections;

namespace com.aurora.aumusic.shared.MessageService
{

    /// <summary>
    /// Main Class of Message Service, it uses Json to transmit messages
    /// </summary>
    public static class MessageService
    {
        // The underlying BMP methods can pass a ValueSet. MessageService
        // relies on this to pass a type and body payload.
        public const string MessageType = "MessageType";
        public const string MessageBody = "MessageBody";

        /// <summary>
        /// a generic method to send message to foreground via BackgroundMediaPlayer
        /// </summary>
        /// <typeparam name="T">a class that has the DataContract attribute</typeparam>
        /// <param name="message">message that need to be transmitted</param>
        public static void SendMessageToForeground<T>(T message)
        {
            ValueSet p = new ValueSet();
            p.Add(MessageType, typeof(T).FullName);
            p.Add(MessageBody, JsonHelper.ToJson(message));
            Windows.Media.Playback.BackgroundMediaPlayer.SendMessageToForeground(p);
        }

        /// <summary>
        /// a generic method to send message to background via BackgroundMediaPlayer
        /// </summary>
        /// <typeparam name="T">a class that has the DataContract attribute</typeparam>
        /// <param name="message">message that need to be transmitted</param>
        public static void SendMessageToBackground<T>(T message)
        {
            ValueSet p = new ValueSet();
            p.Add(MessageType, typeof(T).FullName);
            p.Add(MessageBody, JsonHelper.ToJson(message));
            Windows.Media.Playback.BackgroundMediaPlayer.SendMessageToBackground(p);
        }

        /// <summary>
        /// when received message from the MessageReceivedFromXXX event, this method used to parse message
        /// </summary>
        /// <typeparam name="T">a class that has the DataContract attribute</typeparam>
        /// <param name="valueSet">Data from MediaPlayerDataReceivedEventArgs</param>
        /// <param name="message">target message class</param>
        /// <returns>when true, means message is correctly can be parsed</returns>
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
    }
}
