using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared.MessageService
{
    [DataContract]
    public class BackgroundTaskStateChangedMessage
    {
        public BackgroundTaskStateChangedMessage(BackgroundTaskState state)
        {
            this.TaskState = state;
        }

        [DataMember]
        public BackgroundTaskState TaskState { get; private set; }
    }
}
