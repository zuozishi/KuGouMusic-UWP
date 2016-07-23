using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 酷狗音乐UWP.Class;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace 酷狗音乐UWP.page
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PlayerPgae : Page
    {
        public MediaPlayer BackgroundMedia { get; private set; }
        private DispatcherTimer ProcessTimer = new DispatcherTimer();
        private DispatcherTimer PicTimer = new DispatcherTimer();
        int picnum = 0;
        private Class.Model.Player.NowPlay nowplay;
        private bool ishander = true;
        private Model.SearchResultModel.Singer singerdata;

        public PlayerPgae()
        {
            this.InitializeComponent();
            init();
            //LrcInit();
        }

        private async void init()
        {
            BackgroundMedia = Class.MediaControl.GetCurrent();
            ProcessTimer.Interval = TimeSpan.FromSeconds(60);
            PicTimer.Interval = TimeSpan.FromSeconds(15);
            PicTimer.Tick += (s, e) =>
           {
               if(PlayerFlip.SelectedIndex==1)
               {
                   if (singerdata != null && singerdata.pics != null && singerdata.pics.Count > 0)
                   {
                       if (picnum == singerdata.pics.Count - 1)
                       {
                           picnum = 0;
                       }
                       else
                       {
                           picnum = picnum + 1;
                       }
                       mainGrid.Background = new ImageBrush() { ImageSource = new Windows.UI.Xaml.Media.Imaging.BitmapImage() { UriSource = new Uri(singerdata.pics[picnum].Replace("{size}", "480")) }, Stretch = Stretch.UniformToFill };
                   }
               }
           };
            ProcessTimer.Tick += async (s, e) =>
            {
                ishander = false;
                var alltime = BackgroundMedia.NaturalDuration.TotalSeconds;
                var alltime_m = Math.Floor(alltime / 60);
                var alltime_s = Math.Floor(alltime % 60);
                var thistime = BackgroundMedia.Position.TotalSeconds;
                var thistime_m = Math.Floor(thistime / 60);
                var thistime_s = Math.Floor(thistime % 60);
                if (alltime_s < 10)
                {
                    AllTime_Text.Text = alltime_m.ToString() + ":0" + alltime_s.ToString();
                }
                else
                {
                    AllTime_Text.Text = alltime_m.ToString() + ":" + alltime_s.ToString();
                };
                if (thistime_s < 10)
                {
                    ThisTime_Text.Text = thistime_m.ToString() + ":0" + thistime_s.ToString();
                }
                else
                {
                    ThisTime_Text.Text = thistime_m.ToString() + ":" + thistime_s.ToString();
                };
                SongSlider.Maximum = alltime;
                SongSlider.Value = thistime;
                await Task.Delay(50);
                ishander = true;
            };
            lrcpanel.LrcNowText = NowLrc_Text;
            UpdateData();
            if (BackgroundMedia.CurrentState == MediaPlayerState.Playing)
            {
                PlayBtn.Icon = new SymbolIcon(Symbol.Pause);
                nowplay = await Class.Model.PlayList.GetNowPlay();
                if (nowplay != null)
                {
                    lrcpanel.LrcNowText = NowLrc_Text;
                    lrcpanel.LoadLrcFromLocal();
                }
                ProcessTimer.Start();
            }
            else
            {
                PlayBtn.Icon = new SymbolIcon(Symbol.Play);
            }
            BackgroundMedia.CurrentStateChanged += BackgroundMedia_CurrentStateChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var applicationView = ApplicationView.GetForCurrentView();
                applicationView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = Colors.White;
                statusBar.BackgroundColor = Color.FromArgb(1, 68, 190, 239);
                statusBar.BackgroundOpacity = 0;
                applicationView.TryEnterFullScreenMode();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var applicationView = ApplicationView.GetForCurrentView();
                applicationView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = Colors.White;
                statusBar.BackgroundColor = Color.FromArgb(1, 68, 190, 239);
                statusBar.BackgroundOpacity = 100;
                if(applicationView.IsFullScreenMode)
                {
                    applicationView.ExitFullScreenMode();
                }
            }
        }

        private async void Cyc_Btn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            var icon = btn.Icon as SymbolIcon;
            switch (icon.Symbol)
            {
                case Symbol.RepeatOne:
                    await Class.Model.PlayList.SetCycling(Class.Model.PlayList.cycling.列表循环);
                    Cyc_Btn.Icon = new SymbolIcon(Symbol.RepeatAll);
                    break;
                case Symbol.RepeatAll:
                    await Class.Model.PlayList.SetCycling(Class.Model.PlayList.cycling.随机播放);
                    Cyc_Btn.Icon = new SymbolIcon(Symbol.Shuffle);
                    break;
                case Symbol.Shuffle:
                    await Class.Model.PlayList.SetCycling(Class.Model.PlayList.cycling.单曲循环);
                    Cyc_Btn.Icon = new SymbolIcon(Symbol.RepeatOne);
                    break;
                default: break;
            }
        }

        private void BackgroundMedia_CurrentStateChanged(MediaPlayer sender, object args)
        {
            var currentState = sender.CurrentState;
            switch (currentState)
            {
                case MediaPlayerState.Playing:
                    this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        PlayBtn.Icon = new SymbolIcon(Symbol.Pause);
                        UpdateData();
                        ProcessTimer.Start();
                    });
                    break;
                case MediaPlayerState.Paused:
                    this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        PlayBtn.Icon = new SymbolIcon(Symbol.Play);
                        UpdateData();
                        ProcessTimer.Stop();
                    });
                    break;
                case MediaPlayerState.Stopped:
                    this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        PlayBtn.Icon = new SymbolIcon(Symbol.Play);
                        UpdateData();
                        ProcessTimer.Stop();
                    });
                    break;
                default:
                    break;
            }
        }

        public async void UpdateData()
        {
            var nowplay = await Class.Model.Player.GetNowPlay();
            var playlist = await Class.Model.PlayList.GetPlayList();
            switch (playlist.cyc)
            {
                case Class.Model.PlayList.cycling.单曲循环:
                    Cyc_Btn.Icon = new SymbolIcon(Symbol.RepeatOne);
                    break;
                case Class.Model.PlayList.cycling.列表循环:
                    Cyc_Btn.Icon = new SymbolIcon(Symbol.RepeatAll);
                    break;
                case Class.Model.PlayList.cycling.随机播放:
                    Cyc_Btn.Icon = new SymbolIcon(Symbol.Shuffle);
                    break;
                default:
                    break;
            }
            NowLrc_Text.Text = "";
            if (nowplay != null)
            {
                SongName_Text.Text = nowplay.title;
                SingerName_Text.Text = nowplay.singername;
                
                if (nowplay.albumid.Length > 0)
                {
                    var imgurl = await GetAlbumImg(nowplay.albumid);
                    Album_Img.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage() { UriSource = new Uri(imgurl) };
                }
                else
                {
                    Album_Img.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage() { UriSource = new Uri("ms-appx:///Assets/image/albumimg2.png") };
                }
            }
            else
            {
                SongName_Text.Text = "酷狗音乐";
                SingerName_Text.Text = "传播好音乐";
            }
        }

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var flipview = sender as FlipView;
            var Theme = (Application.Current.Resources.ThemeDictionaries.ToList())[0].Value as ResourceDictionary;
            try
            {
                switch (flipview.SelectedIndex)
                {
                    case 0:
                        Page1_Icon.Fill = new SolidColorBrush(Colors.White);
                        Page2_Icon.Fill = new SolidColorBrush(Color.FromArgb(50, 225, 225, 225));
                        mainGrid.Background = (ImageBrush)Theme["KuGou-Background"];
                        break;
                    case 1:
                        Page1_Icon.Fill = new SolidColorBrush(Color.FromArgb(50, 225, 225, 225));
                        Page2_Icon.Fill = new SolidColorBrush(Colors.White);
                        LoadPics();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                
            }
        }

        private async void LoadPics()
        {
            if(singerdata==null)
            {
                var nowplay = await Class.Model.Player.GetNowPlay();
                singerdata = await Class.Model.SearchResultModel.GetSingerResult(nowplay.singername);
                if (singerdata != null && singerdata.pics != null && singerdata.pics.Count > 0)
                {
                    mainGrid.Background = new ImageBrush() { ImageSource = new Windows.UI.Xaml.Media.Imaging.BitmapImage() { UriSource = new Uri(singerdata.pics[0].Replace("{size}", "480")) }, Stretch = Stretch.UniformToFill };
                    PicTimer.Start();
                }
            }
            else
            {
                if(singerdata != null && singerdata.pics != null && singerdata.pics.Count > 0)
                {
                    mainGrid.Background = new ImageBrush() { ImageSource = new Windows.UI.Xaml.Media.Imaging.BitmapImage() { UriSource = new Uri(singerdata.pics[picnum].Replace("{size}", "480")) }, Stretch = Stretch.UniformToFill };
                }
            }
        }

        private void Back_Btn_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void SearchBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(page.Search));
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            if(BackgroundMedia.CurrentState==MediaPlayerState.Playing)
            {
                BackgroundMedia.Pause();
            }
            else
            {
                if(BackgroundMedia.NaturalDuration.TotalMilliseconds==0)
                {
                    Class.Model.PlayList.Play();
                }
                BackgroundMedia.Play();
            }
        }

        private async Task<string> GetAlbumImg(string Albumid)
        {
            var httpclient = new Noear.UWP.Http.AsyncHttpClient();
            httpclient.Url("http://mobilecdn.kugou.com/api/v3/album/info?albumid=" + Albumid);
            var json = (await httpclient.Get()).GetString();
            var obj = Windows.Data.Json.JsonObject.Parse(json);
            var url = obj.GetNamedObject("data").GetNamedString("imgurl");
            url = url.Replace("{size}", "400");
            return url;
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            Class.Model.PlayList.Next();
        }

        private void PreviousBtn_Click(object sender, RoutedEventArgs e)
        {
            Class.Model.PlayList.Previous();
        }

        private void SongSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (ishander)
            {
                var slider = sender as Slider;
                try
                {
                    BackgroundMedia.Position = TimeSpan.FromSeconds(e.NewValue);
                }
                catch (Exception)
                {

                }
            }
        }

        private void ShareBtn_Clicked(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
