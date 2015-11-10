using com.aurora.aumusic.shared.MessageService;
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
using Windows.Media.Playback;

namespace com.aurora.aumusic.backgroundtask
{
    public class BackgroundAudio : IBackgroundTask
    {
        private SystemMediaTransportControls smtc;
        private BackgroundTaskDeferral deferral;
        private ManualResetEvent backgroundTaskStarted = new ManualResetEvent(false);
        private MediaPlaybackList playbackList = new MediaPlaybackList();
        private bool playbackStartedPreviously = false;
        private const string TrackIdKey = "trackid";

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            smtc = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            smtc.ButtonPressed += Smtc_ButtonPressed;
            smtc.IsEnabled = true;
            smtc.IsPauseEnabled = true;
            smtc.IsPlayEnabled = true;
            smtc.IsNextEnabled = true;
            smtc.IsPreviousEnabled = true;
            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;
        }

        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            //ValueSet p = new ValueSet();
            //p.Add("NowState", NOWPLAYBACKSTATE.Playing);
            //BackgroundMediaPlayer.SendMessageToForeground(p);
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
            playbackList.MovePrevious();

            BackgroundMediaPlayer.Current.Play();
        }

        private void SkipToNext()
        {
            smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
            playbackList.MoveNext();

            BackgroundMediaPlayer.Current.Play();
        }

        private void StartPlayback()
        {
            if (!playbackStartedPreviously)
            {
                playbackStartedPreviously = true;

                // If the task was cancelled we would have saved the current track and its position. We will try playback from there.
                var currentTrackId = ApplicationSettingsHelper.ReadResetSettingsValue("LastPlayed");
                var currentTrackPosition = ApplicationSettingsHelper.ReadResetSettingsValue("LastPlayedPosition");
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
                                var position = TimeSpan.Parse((string)currentTrackPosition);
                                BackgroundMediaPlayer.Current.Position = position;

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
    }
}
