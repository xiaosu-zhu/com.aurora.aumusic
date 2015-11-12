using com.aurora.aumusic.shared;
using com.aurora.aumusic.shared.Albums;
using com.aurora.aumusic.shared.MessageService;
using com.aurora.aumusic.shared.Songs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage.Streams;

namespace com.aurora.aumusic.backgroundtask
{
    public sealed class BackgroundAudio : IBackgroundTask
    {
        private const string TrackIdKey = "trackid";
        private const string TitleKey = "title";
        private const string AlbumArtKey = "albumart";
        private SystemMediaTransportControls smtc;
        private BackgroundTaskDeferral deferral;
        private ManualResetEvent backgroundTaskStarted = new ManualResetEvent(false);
        private MediaPlaybackList playbackList = new MediaPlaybackList();
        private bool playbackStartedPreviously = false;
        private AppState foregroundAppState;
        private NowPlaybackState PlaybackState = NowPlaybackState.Stopped;

        public List<Song> Songs;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            smtc = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            smtc.ButtonPressed += Smtc_ButtonPressed;
            smtc.PropertyChanged += Smtc_PropertyChanged;
            smtc.IsEnabled = true;
            smtc.IsPauseEnabled = true;
            smtc.IsPlayEnabled = true;
            smtc.IsNextEnabled = true;
            smtc.IsPreviousEnabled = true;
            var value = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.AppState);
            if (value == null)
                foregroundAppState = AppState.Unknown;
            else
                foregroundAppState = EnumHelper.Parse<AppState>(value.ToString());
            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;
            if (foregroundAppState != AppState.Suspended)
                MessageService.SendMessageToForeground(new BackgroundTaskStateChangedMessage(BackgroundTaskState.Running));

            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Running.ToString());

            deferral = taskInstance.GetDeferral(); // This must be retrieved prior to subscribing to events below which use it

            // Mark the background task as started to unblock SMTC Play operation (see related WaitOne on this signal)
            backgroundTaskStarted.Set();
            taskInstance.Task.Completed += TaskCompleted;
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
        }

        private void Smtc_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            //TODO:Nothing
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            try
            {
                // immediately set not running
                backgroundTaskStarted.Reset();

                // save state
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId, GetCurrentTrackId() == null ? null : GetCurrentTrackId().ToString());
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.Position, BackgroundMediaPlayer.Current.Position);
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Canceled.ToString());
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, Enum.GetName(typeof(AppState), foregroundAppState));

                // unsubscribe from list changes
                if (playbackList != null)
                {
                    playbackList.CurrentItemChanged -= PlaybackList_CurrentItemChanged;
                    playbackList = null;
                }

                // unsubscribe event handlers
                BackgroundMediaPlayer.MessageReceivedFromForeground -= BackgroundMediaPlayer_MessageReceivedFromForeground;
                smtc.ButtonPressed -= Smtc_ButtonPressed;
                smtc.PropertyChanged -= Smtc_PropertyChanged;

                BackgroundMediaPlayer.Shutdown(); // shutdown media pipeline
            }
            catch (Exception ex)
            {
                throw ex;
            }
            deferral.Complete(); // signals task completion. 
        }

        private void TaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            backgroundTaskStarted.Reset();

            // save state
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId, GetCurrentTrackId() == null ? null : GetCurrentTrackId().ToString());
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.Position, BackgroundMediaPlayer.Current.Position);
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Canceled.ToString());
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, Enum.GetName(typeof(AppState), foregroundAppState));

            // unsubscribe from list changes
            if (playbackList != null)
            {
                playbackList.CurrentItemChanged -= PlaybackList_CurrentItemChanged;
                playbackList = null;
            }

            // unsubscribe event handlers
            BackgroundMediaPlayer.MessageReceivedFromForeground -= BackgroundMediaPlayer_MessageReceivedFromForeground;
            smtc.ButtonPressed -= Smtc_ButtonPressed;
            smtc.PropertyChanged -= Smtc_PropertyChanged;
            BackgroundMediaPlayer.Shutdown(); // shutdown media pipeline
            deferral.Complete();
        }

        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            ForePlaybackChangedMessage message;
            if (MessageService.TryParseMessage(e.Data, out message))
            {
                switch (message.PlaybackState)
                {
                    case DesiredPlaybackState.Play: StartPlayback(); break;
                    case DesiredPlaybackState.Pause: PausePlayback(); break;
                    case DesiredPlaybackState.Next: SkipToNext(); break;
                    case DesiredPlaybackState.Previous: SkipToPrevious(); break;
                    case DesiredPlaybackState.Stop: StopPlayback(); break;
                    default: break;
                }
            }
            UpdatePlaybackMessage update;
            if (MessageService.TryParseMessage(e.Data, out update))
            {
                CreatePlaybackList(update.Albums);
            }
        }

        private void StopPlayback()
        {
            BackgroundMediaPlayer.Current.Pause();
            BackgroundMediaPlayer.Current.Position = TimeSpan.Zero;
            PlaybackState = NowPlaybackState.Stopped;
        }

        private void PausePlayback()
        {
            BackgroundMediaPlayer.Current.Pause();
            PlaybackState = NowPlaybackState.Paused;
        }

        private void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (sender.CurrentState == MediaPlayerState.Playing)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
            }
            else if (sender.CurrentState == MediaPlayerState.Paused)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
            }
            else if (sender.CurrentState == MediaPlayerState.Closed)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Closed;
            }
            if (PlaybackState == NowPlaybackState.Stopped)
                smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
        }

        private void Smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    // When the background task has been suspended and the SMTC
                    // starts it again asynchronously, some time is needed to let
                    // the task startup process in Run() complete.

                    // Wait for task to start. 
                    // Once started, this stays signaled until shutdown so it won't wait
                    // again unless it needs to.
                    bool result = backgroundTaskStarted.WaitOne(5000);
                    if (!result)
                        throw new Exception("Background Task didnt initialize in time");

                    StartPlayback();
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    BackgroundMediaPlayer.Current.Pause();
                    break;
                case SystemMediaTransportControlsButton.Next:
                    SkipToNext();
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    SkipToPrevious();
                    break;
            }
        }

        private void SkipToPrevious()
        {
            smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
            if (BackgroundMediaPlayer.Current.Position.TotalMilliseconds >= BackgroundMediaPlayer.Current.NaturalDuration.TotalMilliseconds / 50)
            {
                BackgroundMediaPlayer.Current.Position = TimeSpan.Zero;
                BackgroundMediaPlayer.Current.Play();
            }
            else
            {
                playbackList.MovePrevious();
                BackgroundMediaPlayer.Current.Play();
            }
        }

        private void SkipToNext()
        {
            smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
            playbackList.MoveNext();
            BackgroundMediaPlayer.Current.Play();
        }

        private void StartPlayback()
        {
            if (PlaybackState == NowPlaybackState.Paused)
            {
                BackgroundMediaPlayer.Current.Play();
            }
            else if (!playbackStartedPreviously)
            {
                playbackStartedPreviously = true;

                // If the task was cancelled we would have saved the current track and its position. We will try playback from there.
                var currentTrackId = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.TrackId);
                var currentTrackPosition = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.Position);
                if (currentTrackId != null)
                {
                    // Find the index of the item by name
                    var index = playbackList.Items.ToList().FindIndex(item =>
                        GetTrackId(item).ToString() == (string)currentTrackId);

                    if (currentTrackPosition == null)
                    {
                        // Play from start if we dont have position
                        playbackList.MoveTo((uint)index);

                        // Begin playing
                        BackgroundMediaPlayer.Current.Play();
                    }
                    else
                    {
                        // Play from exact position otherwise
                        TypedEventHandler<MediaPlaybackList, CurrentMediaPlaybackItemChangedEventArgs> handler = null;
                        handler = (MediaPlaybackList list, CurrentMediaPlaybackItemChangedEventArgs args) =>
                        {
                            if (args.NewItem == playbackList.Items[index])
                            {
                                // Unsubscribe because this only had to run once for this item
                                playbackList.CurrentItemChanged -= handler;

                                // Set position
                                var position = currentTrackPosition;
                                BackgroundMediaPlayer.Current.Position = (TimeSpan)position;

                                // Begin playing
                                BackgroundMediaPlayer.Current.Play();
                            }
                        };
                        playbackList.CurrentItemChanged += handler;

                        // Switch to the track which will trigger an item changed event
                        playbackList.MoveTo((uint)index);
                    }
                }
                else
                {
                    // Begin playing
                    BackgroundMediaPlayer.Current.Play();
                }
            }
            else
            {
                // Begin playing
                BackgroundMediaPlayer.Current.Play();
            }
            PlaybackState = NowPlaybackState.Playing;
        }

        string GetCurrentTrackId()
        {
            if (playbackList == null)
                return null;

            return GetTrackId(playbackList.CurrentItem);
        }

        string GetTrackId(MediaPlaybackItem item)
        {
            if (item == null)
                return null; // no track playing

            return item.Source.CustomProperties[TrackIdKey] as string;
        }

        void CreatePlaybackList(IEnumerable<AlbumItem> albums)
        {
            // Make a new list and enable looping
            playbackList = new MediaPlaybackList();
            playbackList.AutoRepeatEnabled = true;

            // Add playback items to the list
            foreach (var album in albums)
            {
                foreach (var item in album.Songs)
                {
                    var source = MediaSource.CreateFromStorageFile(item.AudioFile);
                    source.CustomProperties[TrackIdKey] = item.MainKey;
                    source.CustomProperties[TitleKey] = item.Title;
                    source.CustomProperties[AlbumArtKey] = new Uri(item.ArtWork);
                    playbackList.Items.Add(new MediaPlaybackItem(source));
                }
                this.Songs.AddRange(album.Songs);
            }

            // Don't auto start
            BackgroundMediaPlayer.Current.AutoPlay = false;

            // Assign the list to the player
            BackgroundMediaPlayer.Current.Source = playbackList;

            // Add handler for future playlist item changes
            playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;
        }

        private void PlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            var item = args.NewItem;
            // Update the system view
            UpdateUVCOnNewTrack(item);

            // Get the current track
            var currentTrackId = item.Source.CustomProperties[TrackIdKey] as string;
            // Notify foreground of change or persist for later
            if (foregroundAppState == AppState.Active)
                MessageService.SendMessageToForeground(new BackPlaybackChangedMessage(PlaybackState, Songs.Find(x => x.MainKey == currentTrackId)));
            else
                ApplicationSettingsHelper.SaveSettingsValue(TrackIdKey, currentTrackId == null ? null : currentTrackId.ToString());

        }

        private void UpdateUVCOnNewTrack(MediaPlaybackItem item)
        {
            if (item == null)
            {
                smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
                smtc.DisplayUpdater.MusicProperties.Title = string.Empty;
                smtc.DisplayUpdater.Update();
                return;
            }

            smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
            smtc.DisplayUpdater.Type = MediaPlaybackType.Music;
            smtc.DisplayUpdater.MusicProperties.Title = item.Source.CustomProperties[TitleKey] as string;

            var albumArtUri = item.Source.CustomProperties[AlbumArtKey] as Uri;
            if (albumArtUri != null)
                smtc.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromUri(albumArtUri);
            else
                smtc.DisplayUpdater.Thumbnail = null;

            smtc.DisplayUpdater.Update();
        }
    }


}
