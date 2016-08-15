using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Json;
using Windows.Data.Xml.Dom;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Notifications;

namespace mediaservice
{
    public sealed class backmedia : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        MediaPlayer mediaplayer = BackgroundMediaPlayer.Current;
        SystemMediaTransportControls stmp= null;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;
            mediaplayer.CurrentStateChanged += Mediaplayer_CurrentStateChanged;
            taskInstance.Canceled += TaskInstance_Canceled;
            stmp = BackgroundMediaPlayer.Current.SystemMediaTransportControls;
            stmp.ButtonPressed += Stmp_ButtonPressed;
            stmp.IsEnabled = true;
            stmp.IsPauseEnabled = true;
            stmp.IsPlayEnabled = true;
            stmp.IsNextEnabled = true;
            stmp.IsPreviousEnabled = true;
            _deferral = taskInstance.GetDeferral();
        }
        private class NowPlay
        {
            public string title { get; set; }
            public string singername { get; set; }
            public string url { get; set; }
            public string imgurl { get; set; }
            public string albumid { get; set; }
            public List<string> pics { get; set; }
        }

        private static async Task<NowPlay> GetNowPlay()
        {
            try
            {
                var datafolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Data");
                var nowplayfile = await datafolder.GetFileAsync("nowplay.json");
                var json = await Windows.Storage.FileIO.ReadTextAsync(nowplayfile);
                var obj = Windows.Data.Json.JsonObject.Parse(json);
                var nowplay = new NowPlay();
                nowplay.title = obj.GetNamedString("title");
                nowplay.singername = obj.GetNamedString("singername");
                nowplay.imgurl= obj.GetNamedString("imgurl");
                return nowplay;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void Stmp_ButtonPressed(Windows.Media.SystemMediaTransportControls sender, Windows.Media.SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case Windows.Media.SystemMediaTransportControlsButton.Play:
                    mediaplayer.Play();
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.Pause:
                    mediaplayer.Pause();
                    break;
                case SystemMediaTransportControlsButton.Next:
                    Next(sender, mediaplayer);
                    break;
                case SystemMediaTransportControlsButton.Previous:
                    Previous(sender, mediaplayer);
                    break;
                default:
                    break;
            }
        }

        private async void Mediaplayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            try
            {
                stmp = SystemMediaTransportControls.GetForCurrentView();
            }
            catch (Exception)
            {
                stmp = mediaplayer.SystemMediaTransportControls;
            }
            var data = sender.CurrentState;
            switch (data)
            {
                case MediaPlayerState.Closed:
                    stmp.IsNextEnabled = true;
                    stmp.IsPreviousEnabled = true;
                    stmp.IsPauseEnabled = false;
                    mediaplayer.Play();
                    stmp.IsPlayEnabled = true;
                    break;
                case MediaPlayerState.Opening:
                    
                    break;
                case MediaPlayerState.Buffering:
                    break;
                case MediaPlayerState.Playing:
                    stmp.IsPauseEnabled = true;
                    stmp.IsNextEnabled = true;
                    stmp.IsPreviousEnabled = true;
                    var datafolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Data");
                    var nowplayfile = await datafolder.GetFileAsync("nowplay.json");
                    var json = await Windows.Storage.FileIO.ReadTextAsync(nowplayfile);
                    var obj = Windows.Data.Json.JsonObject.Parse(json);
                    stmp.DisplayUpdater.ClearAll();
                    stmp.DisplayUpdater.Type = MediaPlaybackType.Music;
                    stmp.DisplayUpdater.MusicProperties.Title = obj.GetNamedString("title");
                    stmp.DisplayUpdater.MusicProperties.Artist = obj.GetNamedString("singername");
                    stmp.DisplayUpdater.Update();
                    stmp.IsPlayEnabled = false;
                    break;
                case MediaPlayerState.Paused:
                    stmp.IsPauseEnabled = true;
                    stmp.IsNextEnabled = true;
                    stmp.IsPreviousEnabled = true;
                    if (Math.Floor(mediaplayer.NaturalDuration.TotalSeconds)-3<=Math.Floor(mediaplayer.Position.TotalSeconds))
                    {
                        Next(stmp, mediaplayer);
                    }
                    stmp.IsPlayEnabled = true;
                    break;
                case MediaPlayerState.Stopped:
                    stmp.IsPauseEnabled = false;
                    stmp.IsNextEnabled = true;
                    stmp.IsPreviousEnabled = true;
                    mediaplayer.Play();
                    stmp.IsPlayEnabled = true;
                    break;
                default:
                    break;
            }
        }

        private async void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            var data = e.Data.ToList();
            var path = data[0].Value.ToString();
            if (path.Contains("http://"))
            {
                mediaplayer.SetUriSource(new Uri(path));
            }
            else
            {
                var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(path);
                mediaplayer.SetFileSource(file);
            }
            UpdateTitle();
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if(_deferral!=null)
            {
                _deferral.Complete();
            }
        }

        private class PlayList
        {
            public int nowplay { get; set; }
            public List<NowPlay> SongList { get; set; }
            public cycling cyc { get; set; }
            public enum cycling
            {
                单曲循环, 列表循环, 随机播放
            }
            public static async Task<PlayList> GetPlayList()
            {
                try
                {
                    var list = new PlayList();
                    var datafolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Data");
                    var playlistfile = await datafolder.GetFileAsync("playlist.json");
                    var json = await Windows.Storage.FileIO.ReadTextAsync(playlistfile);
                    list = DataContractJsonDeSerialize(json);
                    return list;
                }
                catch (Exception)
                {
                    return null;
                }
            }

            public static PlayList DataContractJsonDeSerialize(string json)
            {
                try
                {
                    var obj = JsonObject.Parse(json);
                    var list = new PlayList();
                    list.nowplay = int.Parse(obj["nowplay"].GetNumber().ToString());
                    var cyc = obj["cyc"].GetNumber().ToString();
                    switch (cyc)
                    {
                        case "0":list.cyc = cycling.单曲循环;break;
                        case "1": list.cyc = cycling.列表循环; break;
                        case "2": list.cyc = cycling.随机播放; break;
                        default:break;
                    }
                    list.SongList = new List<NowPlay>();
                    try
                    {
                        var songs = obj["SongList"].GetArray();
                        foreach (var song in songs)
                        {
                            var nowplay = new NowPlay();
                            nowplay.albumid = song.GetObject()["albumid"].GetString();
                            nowplay.title = song.GetObject()["title"].GetString();
                            nowplay.singername = song.GetObject()["singername"].GetString();
                            nowplay.url = song.GetObject()["url"].GetString();
                            nowplay.imgurl = song.GetObject()["imgurl"].GetString();
                            nowplay.pics = new List<string>();
                            try
                            {
                                var pics = song.GetObject()["pics"].GetArray();
                                foreach (var pic in pics)
                                {
                                    nowplay.pics.Add(pic.GetString());
                                }
                            }
                            catch (Exception)
                            {

                            }
                            list.SongList.Add(nowplay);
                        }
                    }
                    catch (Exception)
                    {
                        
                    }
                    return list;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            public static string ToJsonData(NowPlay nowplay)
            {
                var obj = new JsonObject();
                obj.Add("title", JsonValue.CreateStringValue(nowplay.title));
                obj.Add("url", JsonValue.CreateStringValue(nowplay.url));
                obj.Add("imgurl", JsonValue.CreateStringValue(nowplay.imgurl));
                obj.Add("albumid", JsonValue.CreateStringValue(nowplay.albumid));
                obj.Add("singername", JsonValue.CreateStringValue(nowplay.singername));
                try
                {
                    var num = nowplay.pics.Count;
                    var arry = new JsonArray();
                    foreach (var pic in nowplay.pics)
                    {
                        arry.Add(JsonValue.CreateStringValue(pic));
                    }
                    obj.Add("pics", arry);
                }
                catch (Exception)
                {
                    
                }
                var result = obj.ToString();
                return result;
            }
            public static string ToJsonData(PlayList playlist)
            {
                var obj = new JsonObject();
                obj.Add("nowplay", JsonValue.CreateNumberValue(int.Parse(playlist.nowplay.ToString())));
                switch (playlist.cyc)
                {
                    case cycling.单曲循环:
                        obj.Add("cyc", JsonValue.CreateNumberValue(0));
                        break;
                    case cycling.列表循环:
                        obj.Add("cyc", JsonValue.CreateNumberValue(1));
                        break;
                    case cycling.随机播放:
                        obj.Add("cyc", JsonValue.CreateNumberValue(2));
                        break;
                    default:
                        break;
                }
                try
                {
                    var arry = new JsonArray();
                    foreach (var item in playlist.SongList)
                    {
                        arry.Add(JsonValue.Parse(ToJsonData(item)));
                    }
                    obj.Add("SongList", arry);
                }
                catch (Exception)
                {
                    
                }
                var json = obj.ToString();
                return json;
            }
        }

        private static async void Next(SystemMediaTransportControls stmp,MediaPlayer mediaplayer)
        {
            try
            {
                var list = await PlayList.GetPlayList();
                var datafolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Data");
                var nowplayfile = await datafolder.GetFileAsync("nowplay.json");
                var playlistfile = await datafolder.GetFileAsync("playlist.json");
                var now = list.nowplay;
                switch (list.cyc)
                {
                    case PlayList.cycling.单曲循环:
                        await Windows.Storage.FileIO.WriteTextAsync(nowplayfile, PlayList.ToJsonData(list.SongList[now]));
                        stmp.DisplayUpdater.ClearAll();
                        stmp.DisplayUpdater.Type = MediaPlaybackType.Music;
                        stmp.DisplayUpdater.MusicProperties.Title = list.SongList[now].title;
                        stmp.DisplayUpdater.MusicProperties.Artist = list.SongList[now].singername;
                        stmp.DisplayUpdater.Update();
                        if (list.SongList[now].url.Contains("http"))
                        {
                            mediaplayer.SetUriSource(new Uri(list.SongList[now].url));
                        }
                        else
                        {
                            mediaplayer.SetFileSource(await Windows.Storage.StorageFile.GetFileFromPathAsync(list.SongList[now].url));
                        }
                        UpdateTitle();
                        break;
                    case PlayList.cycling.列表循环:
                        if (now == list.SongList.Count - 1)
                        {
                            now = 0;
                        }
                        else
                        {
                            now = now + 1;
                        }
                        list.nowplay = now;
                        await Windows.Storage.FileIO.WriteTextAsync(nowplayfile, PlayList.ToJsonData(list.SongList[now]));
                        stmp.DisplayUpdater.ClearAll();
                        stmp.DisplayUpdater.Type = MediaPlaybackType.Music;
                        stmp.DisplayUpdater.MusicProperties.Title = list.SongList[now].title;
                        stmp.DisplayUpdater.MusicProperties.Artist = list.SongList[now].singername;
                        stmp.DisplayUpdater.Update();
                        if (list.SongList[now].url.Contains("http"))
                        {
                            mediaplayer.SetUriSource(new Uri(list.SongList[now].url));
                        }
                        else
                        {
                            mediaplayer.SetFileSource(await Windows.Storage.StorageFile.GetFileFromPathAsync(list.SongList[now].url));
                        }
                        await Windows.Storage.FileIO.WriteTextAsync(playlistfile, PlayList.ToJsonData(list));
                        UpdateTitle();
                        break;
                    case PlayList.cycling.随机播放:
                            now = new Random().Next(0, list.SongList.Count);
                            list.nowplay = now;
                            await Windows.Storage.FileIO.WriteTextAsync(nowplayfile, PlayList.ToJsonData(list.SongList[now]));
                            stmp.DisplayUpdater.ClearAll();
                            stmp.DisplayUpdater.Type = MediaPlaybackType.Music;
                            stmp.DisplayUpdater.MusicProperties.Title = list.SongList[now].title;
                            stmp.DisplayUpdater.MusicProperties.Artist = list.SongList[now].singername;
                            stmp.DisplayUpdater.Update();
                            if (list.SongList[now].url.Contains("http"))
                            {
                                mediaplayer.SetUriSource(new Uri(list.SongList[now].url));
                            }
                            else
                            {
                                mediaplayer.SetFileSource(await Windows.Storage.StorageFile.GetFileFromPathAsync(list.SongList[now].url));
                            }
                        await Windows.Storage.FileIO.WriteTextAsync(playlistfile, PlayList.ToJsonData(list));
                        UpdateTitle();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {

            }
        }
        private static async void Previous(SystemMediaTransportControls stmp, MediaPlayer mediaplayer)
        {
            try
            {
                var list = await PlayList.GetPlayList();
                var now = list.nowplay;
                var datafolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Data");
                var nowplayfile = await datafolder.GetFileAsync("nowplay.json");
                var playlistfile = await datafolder.GetFileAsync("playlist.json");
                switch (list.cyc)
                {
                    case PlayList.cycling.单曲循环:
                        await Windows.Storage.FileIO.WriteTextAsync(nowplayfile, PlayList.ToJsonData(list.SongList[now]));
                        stmp.DisplayUpdater.ClearAll();
                        stmp.DisplayUpdater.Type = MediaPlaybackType.Music;
                        stmp.DisplayUpdater.MusicProperties.Title = list.SongList[now].title;
                        stmp.DisplayUpdater.MusicProperties.Artist = list.SongList[now].singername;
                        stmp.DisplayUpdater.Update();
                        if (list.SongList[now].url.Contains("http"))
                        {
                            mediaplayer.SetUriSource(new Uri(list.SongList[now].url));
                        }
                        else
                        {
                            mediaplayer.SetFileSource(await Windows.Storage.StorageFile.GetFileFromPathAsync(list.SongList[now].url));
                        }
                        UpdateTitle();
                        break;
                    case PlayList.cycling.列表循环:
                        if (now == 0)
                        {
                            now = list.SongList.Count-1;
                        }
                        else
                        {
                            now = now - 1;
                        }
                        list.nowplay = now;
                        await Windows.Storage.FileIO.WriteTextAsync(nowplayfile, PlayList.ToJsonData(list.SongList[now]));
                        stmp.DisplayUpdater.ClearAll();
                        stmp.DisplayUpdater.Type = MediaPlaybackType.Music;
                        stmp.DisplayUpdater.MusicProperties.Title = list.SongList[now].title;
                        stmp.DisplayUpdater.MusicProperties.Artist = list.SongList[now].singername;
                        stmp.DisplayUpdater.Update();
                        if (list.SongList[now].url.Contains("http"))
                        {
                            mediaplayer.SetUriSource(new Uri(list.SongList[now].url));
                        }
                        else
                        {
                            mediaplayer.SetFileSource(await Windows.Storage.StorageFile.GetFileFromPathAsync(list.SongList[now].url));
                        }
                        await Windows.Storage.FileIO.WriteTextAsync(playlistfile, PlayList.ToJsonData(list));
                        UpdateTitle();
                        break;
                    case PlayList.cycling.随机播放:
                        now = new Random().Next(0, list.SongList.Count);
                        list.nowplay = now;
                        await Windows.Storage.FileIO.WriteTextAsync(nowplayfile, PlayList.ToJsonData(list.SongList[now]));
                        stmp.DisplayUpdater.ClearAll();
                        stmp.DisplayUpdater.Type = MediaPlaybackType.Music;
                        stmp.DisplayUpdater.MusicProperties.Title = list.SongList[now].title;
                        stmp.DisplayUpdater.MusicProperties.Artist = list.SongList[now].singername;
                        stmp.DisplayUpdater.Update();
                        if (list.SongList[now].url.Contains("http"))
                        {
                            mediaplayer.SetUriSource(new Uri(list.SongList[now].url));
                        }
                        else
                        {
                            mediaplayer.SetFileSource(await Windows.Storage.StorageFile.GetFileFromPathAsync(list.SongList[now].url));
                        }
                        await Windows.Storage.FileIO.WriteTextAsync(playlistfile, PlayList.ToJsonData(list));
                        UpdateTitle();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {

            }
        }

        private static async void UpdateTitle()
        {
            var datafolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Data");
            var nowplayfile = await datafolder.GetFileAsync("nowplay.json");
            var json = await Windows.Storage.FileIO.ReadTextAsync(nowplayfile);
            datafolder = await Windows.Storage.KnownFolders.MusicLibrary.CreateFolderAsync("kugoudata", CreationCollisionOption.OpenIfExists);
            nowplayfile=await datafolder.CreateFileAsync("nowplay.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(nowplayfile, json);
        }
    }
}
