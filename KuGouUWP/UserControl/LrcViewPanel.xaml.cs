using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Playback;
using KuGouUWP.Class;
using Windows.Storage;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace KuGouUWP.UserControlClass
{
    public sealed partial class LrcViewPanel : UserControl
    {
        public Class.Lrc LrcData { get; set; }
        public DispatcherTimer LrcUpdateTimer { get; set; }
        public List<TimeSpan> LrcTimeLines { get; set; }
        public TextBlock LrcNowText { get; set; }
        public Frame mainFrame { get; private set; }

        public LrcViewPanel()
        {
            this.InitializeComponent();
            LrcTimeLines = new List<TimeSpan>();
            LrcUpdateTimer = new DispatcherTimer();
            LrcUpdateTimer.Interval = TimeSpan.FromMilliseconds(500);
            nolrcview.Visibility = Visibility.Visible;
            mainFrame = Window.Current.Content as Frame;
            Class.MediaControl.GetCurrent().CurrentStateChanged += LrcViewPanel_CurrentStateChanged;
        }

        private void LrcViewPanel_CurrentStateChanged(MediaPlayer sender, object args)
        {
            mainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                switch (sender.CurrentState)
                {
                    case MediaPlayerState.Playing:
                        LoadLrcFromLocal();
                        LrcUpdateTimer.Start();
                        break;
                    case MediaPlayerState.Paused:
                        LoadLrcFromLocal();
                        LrcUpdateTimer.Stop();
                        break;
                    default:
                        break;
                }
            });
        }

        public async void LoadLrcFromSongName(string songname, string duration)
        {
            try
            {
                var lrcfile = await (await KnownFolders.MusicLibrary.GetFolderAsync("kugou_lrc")).CreateFileAsync(songname + ".lrc");
                try
                {
                    var httpclient = new System.Net.Http.HttpClient();
                    var json= await httpclient.GetStringAsync("http://lyrics.kugou.com/search?ver=1&man=yes&client=pc&keyword=" + songname + "&duration=" + duration);
                    var arry = Windows.Data.Json.JsonObject.Parse(json).GetNamedArray("candidates");
                    if (arry.Count != 0)
                    {
                        var id = arry[0].GetObject().GetNamedString("id");
                        var accesskey = arry[0].GetObject().GetNamedString("accesskey");
                        LoadLrcFormId(id, accesskey, songname);
                    }
                    else
                    {
                        await FileIO.WriteTextAsync(lrcfile, "暂无歌词");
                        nolrcview.Visibility = Visibility.Visible;
                        lrcdatapanel.Visibility = Visibility.Collapsed;
                        LoadLrcFromLocal();
                    }
                }
                catch (Exception)
                {
                    await FileIO.WriteTextAsync(lrcfile, "暂无歌词");
                    nolrcview.Visibility = Visibility.Visible;
                    lrcdatapanel.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
                
            }
        }

        public async void LoadLrcFormId(string id,string accesskey,string filename)
        {
            try
            {
                var httpclient = new System.Net.Http.HttpClient();
                var json = await httpclient.GetStringAsync("http://lyrics.kugou.com/download?ver=1&client=pc&id=" + id + "&accesskey=" + accesskey + "&fmt=lrc&charset=utf8");
                var content = Windows.Data.Json.JsonObject.Parse(json).GetNamedString("content");
                var data = Convert.FromBase64String(content);
                json = System.Text.ASCIIEncoding.UTF8.GetString(data);
                var lrcfolder = await KnownFolders.MusicLibrary.GetFolderAsync("kugou_lrc");
                try
                {
                    var file = await lrcfolder.CreateFileAsync(filename + ".lrc");
                    await FileIO.WriteTextAsync(file, json);
                    LoadLrcFromLocal();
                }
                catch (Exception)
                {
                    var file = await lrcfolder.GetFileAsync(filename + ".lrc");
                    await FileIO.WriteTextAsync(file, json);
                    LoadLrcFromLocal();
                }
            }
            catch (Exception)
            {
                var lrcfolder = await KnownFolders.MusicLibrary.GetFolderAsync("kugou_lrc");
                try
                {
                    var file = await lrcfolder.CreateFileAsync(filename + ".lrc");
                    await FileIO.WriteTextAsync(file, "暂无歌词");
                    LoadLrcFromLocal();
                }
                catch (Exception)
                {
                    var file = await lrcfolder.GetFileAsync(filename + ".lrc");
                    await FileIO.WriteTextAsync(file, "暂无歌词");
                    LoadLrcFromLocal();
                }
            }
        }

        public async void LoadLrcFromLocal()
        {
            var nowplay = await Class.Model.Player.GetNowPlay();
            if(nowplay!=null)
            {
                try
                {
                    var lrcfolder = await KnownFolders.MusicLibrary.GetFolderAsync("kugou_lrc");
                    try
                    {
                        var lrcfiles = await lrcfolder.GetFileAsync(nowplay.title + "-" + nowplay.singername + ".lrc");
                        var json = await FileIO.ReadLinesAsync(lrcfiles);
                        if (json.Count > 0)
                        {
                            if (json[0] == "暂无歌词")
                            {
                                nolrcview.Visibility = Visibility.Visible;
                                lrcdatapanel.Visibility = Visibility.Collapsed;
                                try
                                {
                                    LrcUpdateTimer.Tick -= LrcUpdateTimer_Tick;
                                }
                                catch (Exception)
                                {
                                    
                                }
                            }
                            else
                            {
                                try
                                {
                                    LrcUpdateTimer.Tick += LrcUpdateTimer_Tick;
                                }
                                catch (Exception)
                                {
                                    
                                }
                                nolrcview.Visibility = Visibility.Collapsed;
                                lrcdatapanel.Visibility = Visibility.Visible;
                                var lrcdata = Class.Lrc.InitLrc(json.ToList());
                                LoadLrc(lrcdata);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        LoadLrcFromSongName(nowplay.title + "-" + nowplay.singername, Math.Floor(BackgroundMediaPlayer.Current.NaturalDuration.TotalSeconds).ToString() + "000");
                    }
                }
                catch (Exception)
                {
                    await Windows.Storage.KnownFolders.MusicLibrary.CreateFolderAsync("kugou_lrc");
                    LoadLrcFromLocal();
                }
            }
        }

        private void LoadLrc(Lrc lrcdata)
        {
            LrcData = lrcdata;
            lrcdatapanel.Children.Clear();
            LrcTimeLines.Clear();
            foreach (var item in lrcdata.LrcWord)
            {
                var textblock = new TextBlock();
                textblock.Style = (Style)Resources["unlrcview"];
                textblock.Text = item.Value;
                LrcTimeLines.Add(TimeSpan.FromMilliseconds(item.Key));
                lrcdatapanel.Children.Add(textblock);
            }
            if(Class.MediaControl.GetCurrent().CurrentState==Windows.Media.Playback.MediaPlayerState.Playing)
            {
                LrcUpdateTimer.Start();
            }
        }

        private void LrcUpdateTimer_Tick(object sender, object e)
        {
            var thistime = MediaControl.GetCurrent().Position.TotalMilliseconds;
            var time = TimeSpan.FromMilliseconds(thistime);
            for (int i = 0; i < LrcTimeLines.Count; i++)
            {
                if (time.CompareTo(LrcTimeLines[i]) == 1)
                {
                    if(LrcNowText!=null)
                    {
                        LrcNowText.Text =(LrcData.LrcWord.Values.ToList())[i];
                    }
                    foreach (var item in lrcdatapanel.Children)
                    {
                        ((TextBlock)item).Style = (Style)Resources["unlrcview"];
                    }
                    var text = lrcdatapanel.Children[i] as TextBlock;
                    text.Style=(Style)Resources["islrcview"];
                }
            }
        }
    }
}
