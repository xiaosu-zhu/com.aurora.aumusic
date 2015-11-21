using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared.MessageService
{

    public enum BackgroundTaskState
    {
        Unknown,
        Running,
        Canceled,
        Stopped
    }

    public enum PlaybackState { Playing, Paused, Next, Previous, Stopped, Unknown };
    public enum AppState { Active, Suspended, Unknown };
}
