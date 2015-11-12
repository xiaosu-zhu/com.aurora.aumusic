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

    public enum DesiredPlaybackState { Play, Pause, Next, Previous, Stop, Unknown };
    public enum NowPlaybackState { Playing, Paused, Stopped, Previous, Next };
    public enum AppState { Active, Suspended, Unknown };
}
