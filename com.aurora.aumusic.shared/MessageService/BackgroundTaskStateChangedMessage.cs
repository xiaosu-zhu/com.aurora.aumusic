using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared.MessageService
{
    public class BackgroundTaskStateChangedMessage
    {
        public BackgroundTaskStateChangedMessage(BackgroundTaskState state)
        {
            this.TaskState = state;
        }

        public BackgroundTaskState TaskState { get; private set; }
    }
}
