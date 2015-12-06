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
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.System.Threading;

namespace com.aurora.aumusic.backgroundtask
{
    public sealed class BackgroundAudio : IBackgroundTask
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private const string TrackIdKey = "trackid";
        private const string TitleKey = "title";
        private const string AlbumArtKey = "albumart";
        private const string AlbumKey = "album";
        private SystemMediaTransportControls smtc;
        private BackgroundTaskDeferral deferral;
        private ManualResetEvent backgroundTaskStarted = new ManualResetEvent(false);
        private MediaPlaybackList playbackList = new MediaPlaybackList();
        private bool playbackStartedPreviously = false;
        private AppState foregroundAppState;
        private MediaPlayerState NowState = MediaPlayerState.Stopped;
        private MediaPlaybackList templist;

        List<KeyValuePair<string, List<IStorageFile>>> AllList = new List<KeyValuePair<string, List<IStorageFile>>>();

        private List<SongModel> Songs;

        private List<IStorageFile> FileList = new List<IStorageFile>();
        private PlaybackMode NowMode;

        private TimeSpan CurrentPosition = TimeSpan.Zero;

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
            ThreadPool.RunAsync(async (work) =>
            {
                if (!(bool)localSettings.Values["isCreated"])
                {
                    MessageService.SendMessageToForeground(new RefreshStateMessage(RefreshState.NeedRefresh));
                    return;
                }
                ApplicationDataCompositeValue composite = (ApplicationDataCompositeValue)localSettings.Values["FolderSettings"];
                int count = (int)composite["FolderCount"];
                for (int i = 0; i < count; i++)
                {
                    List<IStorageFile> files = new List<IStorageFile>();
                    string tempPath = (string)composite["FolderSettings" + i.ToString()];
                    try
                    {
                        StorageFolder folder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(tempPath);
                        files.AddRange(await AlbumEnum.SearchAllinFolder(folder));
                        AllList.Add(new KeyValuePair<string, List<IStorageFile>>(tempPath, files));
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }
                backgroundTaskStarted.Set();
                MessageService.SendMessageToForeground(new BackgroundConfirmFilesMessage());
            });

            // Mark the background task as started to unblock SMTC Play operation (see related WaitOne on this signal)

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
                TaskExecute();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            deferral.Complete(); // signals task completion. 
        }

        private void TaskCompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
        }

        private void TaskExecute()
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
        }

        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            ForePlaybackChangedMessage message;
            if (MessageService.TryParseMessage(e.Data, out message))
            {
                if (message.DesiredSongs != null)
                    CreatePlaybackList(message.DesiredSongs);
                switch (message.DesiredPlaybackState)
                {
                    case PlaybackState.Playing: StartPlayback(message.Index); break;
                    case PlaybackState.Paused: PausePlayback(); break;
                    case PlaybackState.Next: SkipToNext(); break;
                    case PlaybackState.Previous: SkipToPrevious(); break;
                    case PlaybackState.Stopped: StopPlayback(); break;
                    case PlaybackState.Unknown: break;
                }
                var time = BackgroundMediaPlayer.Current.Position;
                switch (message.DesiredPlaybackMode)
                {
                    case PlaybackMode.Normal:
                        ResetplaybackList();
                        playbackList.ShuffleEnabled = false;
                        playbackList.AutoRepeatEnabled = false;
                        BackgroundMediaPlayer.Current.Position = time;
                        if (NowState == MediaPlayerState.Playing)
                        {
                            BackgroundMediaPlayer.Current.Play();
                        }
                        break;
                    case PlaybackMode.Repeat:
                        playbackList.ShuffleEnabled = false;
                        playbackList.AutoRepeatEnabled = true;
                        BackgroundMediaPlayer.Current.Position = time;
                        if (NowState == MediaPlayerState.Playing)
                        {
                            BackgroundMediaPlayer.Current.Play();
                        }
                        break;
                    case PlaybackMode.RepeatSingle:
                        SetplaybackListSingle();
                        playbackList.ShuffleEnabled = false;
                        playbackList.AutoRepeatEnabled = true;
                        BackgroundMediaPlayer.Current.Position = time;
                        if (NowState == MediaPlayerState.Playing)
                        {
                            BackgroundMediaPlayer.Current.Play();
                        }
                        break;
                    case PlaybackMode.Shuffle:
                        playbackList.ShuffleEnabled = true;
                        playbackList.AutoRepeatEnabled = false;
                        BackgroundMediaPlayer.Current.Position = time;
                        if (NowState == MediaPlayerState.Playing)
                        {
                            BackgroundMediaPlayer.Current.Play();
                        }
                        break;
                    case PlaybackMode.ShuffleRepeat:
                        playbackList.AutoRepeatEnabled = true;
                        playbackList.ShuffleEnabled = false;
                        BackgroundMediaPlayer.Current.Position = time;
                        if (NowState == MediaPlayerState.Playing)
                        {
                            BackgroundMediaPlayer.Current.Play();
                        }
                        break;
                    default:
                        break;
                }
            }
            UpdatePlaybackMessage update;
            if (MessageService.TryParseMessage(e.Data, out update))
            {
                ConfirmFiles(update.Songs);
            }
            NeedNowListMessage nowlist;
            if (MessageService.TryParseMessage(e.Data, out nowlist))
            {
                MessageService.SendMessageToForeground(new NowListMessage(Songs));
            }
            NeedFullFileDetailsMessage detail;
            if (MessageService.TryParseMessage(e.Data, out detail))
            {
                ThreadPool.RunAsync((work) =>
                {
                    FindCurrentFile(detail.MainKey);
                });
            }
        }

        private async void FindCurrentFile(string mainkey)
        {
            var file = FileList.Find(x => (mainkey == (((StorageFile)x).Path)));
            MusicProperties p = await ((StorageFile)file).Properties.GetMusicPropertiesAsync();
            var b = await ((StorageFile)file).GetBasicPropertiesAsync();
            var fulldetail = new FullFileDetailsMessage(file.FileType, b.Size, p.Bitrate);
            MessageService.SendMessageToForeground(fulldetail);
        }

        private void ResetplaybackList()
        {
            playbackList.CurrentItemChanged -= PlaybackList_CurrentItemChanged;
            var tem = playbackList.CurrentItem;
            if (templist != null)
            {
                foreach (var item in templist.Items)
                {
                    playbackList.Items.Add(item);
                }
                var index = playbackList.Items.IndexOf(tem);
                playbackList.MoveTo((uint)index);
            }
            playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;
        }

        private void SetplaybackListSingle()
        {
            playbackList.CurrentItemChanged -= PlaybackList_CurrentItemChanged;
            templist = new MediaPlaybackList();
            foreach (var item in playbackList.Items)
            {
                templist.Items.Add(item);
            }
            var current = playbackList.CurrentItem;
            playbackList.Items.Clear();
            playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;
            playbackList.Items.Add(current);
        }

        private void CreatePlaybackList(List<SongModel> desiredSongs)
        {
            playbackList.CurrentItemChanged -= PlaybackList_CurrentItemChanged;
            playbackList.Items.Clear();
            int i = 0;
            while (true)
            {
                if (i >= desiredSongs.Count)
                    break;
                var file = FileList.Find(x => (desiredSongs[i].MainKey == (((StorageFile)x).Path)));
                if (file == null)
                {
                    MessageService.SendMessageToForeground(new FileNotFindMessage(desiredSongs[i].MainKey));
                    desiredSongs.RemoveAt(i);
                    i++;
                    continue;
                }
                var source = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(file));
                source.Source.CustomProperties[TrackIdKey] = desiredSongs[i].MainKey;
                source.Source.CustomProperties[AlbumArtKey] = desiredSongs[i].AlbumArtwork;
                source.Source.CustomProperties[TitleKey] = desiredSongs[i].Title;
                source.Source.CustomProperties[AlbumKey] = desiredSongs[i].Album;
                playbackList.Items.Add(source);
                i++;
            }
            this.Songs = desiredSongs;
            playbackList.AutoRepeatEnabled = false;
            BackgroundMediaPlayer.Current.AutoPlay = false;
            BackgroundMediaPlayer.Current.Source = playbackList;
            playbackList.ItemFailed += PlaybackList_ItemFailed;
            playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;
        }

        private void PlaybackList_ItemFailed(MediaPlaybackList sender, MediaPlaybackItemFailedEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void StopPlayback()
        {
            BackgroundMediaPlayer.Current.Pause();
            BackgroundMediaPlayer.Current.Position = TimeSpan.Zero;
            CurrentPosition = TimeSpan.Zero;
            NowState = MediaPlayerState.Stopped;
        }

        private void PausePlayback()
        {
            CurrentPosition = BackgroundMediaPlayer.Current.Position;
            BackgroundMediaPlayer.Current.Pause();
            NowState = MediaPlayerState.Paused;
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
            if (NowState == MediaPlayerState.Stopped)
                smtc.PlaybackStatus = MediaPlaybackStatus.Stopped;
        }

        private void Smtc_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    bool result = backgroundTaskStarted.WaitOne(5000);
                    if (!result)
                        throw new Exception("Background Task didnt initialize in time");

                    StartPlayback(null);
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
                CurrentPosition = TimeSpan.Zero;
                BackgroundMediaPlayer.Current.Play();
            }
            else
            {
                playbackList.MovePrevious();
                if (NowState == MediaPlayerState.Playing)
                    BackgroundMediaPlayer.Current.Play();
                else BackgroundMediaPlayer.Current.Pause();
            }
        }

        private void SkipToNext()
        {
            smtc.PlaybackStatus = MediaPlaybackStatus.Changing;
            playbackList.MoveNext();
            if (NowState == MediaPlayerState.Playing)
                BackgroundMediaPlayer.Current.Play();
            else BackgroundMediaPlayer.Current.Pause();
        }

        private async void StartPlayback(SongModel song)
        {
            if (song != null)
            {
                var index = playbackList.Items.ToList().FindIndex(item =>
                            GetTrackId(item).ToString() == song.MainKey);
                try
                {
                    playbackList.MoveTo((uint)index);
                }
                catch (Exception)
                {
                    await Task.Delay(1000);
                    playbackList.MoveTo((uint)index);
                }

            }
            BackgroundMediaPlayer.Current.Play();
            BackgroundMediaPlayer.Current.Position = TimeSpan.Zero;
            NowState = MediaPlayerState.Playing;
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

        async void ConfirmFiles(IEnumerable<SongModel> mainkeys)
        {
            // Make a new list and enable looping

            // Add playback items to the list
            var result = backgroundTaskStarted.WaitOne(10000);
            if (result == true)
                for (int k = AllList.Count - 1; k >= 0; k--)
                {
                    FileList.Clear();
                    FileList.AddRange(AllList[k].Value);
                    for (int j = AllList[k].Value.Count - 1; j >= 0; j--)
                    {
                        foreach (var key in mainkeys)
                        {
                            if (key.MainKey == (((StorageFile)AllList[k].Value[j]).Path))
                            {

                                AllList[k].Value.RemoveAt(j);
                                break;
                            }
                        }
                    }
                    if (AllList[k].Value.Count == 0)
                        AllList.RemoveAt(k);
                }
            if (AllList.Count > 0)
                MessageService.SendMessageToForeground(new RefreshStateMessage(RefreshState.NeedRefresh));
            AllList = null;
        }

        private async void PlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            var item = args.NewItem;
            // Update the system view
            if (item == null)
                return;
            UpdateUVCOnNewTrack(item);

            // Get the current track
            var currentTrackId = item.Source.CustomProperties[TrackIdKey] as string;
            // Notify foreground of change or persist for later
            MessageService.SendMessageToForeground(new BackPlaybackChangedMessage(NowState, Songs.Find(x => x.MainKey == currentTrackId)));
            ThreadPool.RunAsync(async (work) =>
            {
                var bytestream = await FileHelper.FetchArtwork(FileList.Find(x => currentTrackId == ((StorageFile)x).Path));
                if (bytestream != null)
                {
                    MessageService.SendMessageToForeground(new UpdateArtworkMessage(bytestream));
                    UpdateUVCOnNewTrack(bytestream);
                }
            });
        }

        private async void UpdateUVCOnNewTrack(byte[] bytestream)
        {
            smtc.DisplayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromStream(await FileHelper.ToRandomAccessStream(bytestream));
            smtc.DisplayUpdater.Update();
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

            var albumArtUri = new Uri(item.Source.CustomProperties[AlbumArtKey] as string);
            smtc.DisplayUpdater.Update();
        }
    }


}
