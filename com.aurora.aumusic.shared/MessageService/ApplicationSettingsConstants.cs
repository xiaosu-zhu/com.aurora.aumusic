using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.aurora.aumusic.shared.MessageService
{
    public static class ApplicationSettingsConstants
    {
        // Data keys
        public const string TrackId = "trackid";
        public const string Position = "position";
        public const string BackgroundTaskState = "backgroundtaskstate"; // Started, Running, Cancelled
        public const string AppState = "appstate"; // Suspended, Resumed
        public const string AppSuspendedTimestamp = "appsuspendedtimestamp";
        public const string AppResumedTimestamp = "appresumedtimestamp";
    }
}
