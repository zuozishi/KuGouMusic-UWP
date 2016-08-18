using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using KuGouUWP.Class;
using ImageUtility;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.DataTransfer;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace KuGouUWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PlayerPgae : Page
    {
        public MediaPlayer BackgroundMedia { get; private set; }
        private DispatcherTimer ProcessTimer = new DispatcherTimer();
        private DispatcherTimer PicTimer = new DispatcherTimer();
        private ImageBrush albumimgbrush;
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
            ProcessTimer.Interval = TimeSpan.FromMilliseconds(800);
            PicTimer.Interval = TimeSpan.FromSeconds(60);
            PicTimer.Tick += async (s, e) =>
           {
               if (PlayerFlip.SelectedIndex == 1)
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
                       if (PlayerFlip.SelectedIndex == 1)
                       {
                           WriteableBitmap wb = new WriteableBitmap(1000, 1500);
                           HttpClient hc = new HttpClient();
                           byte[] b = await hc.GetByteArrayAsync(singerdata.pics[picnum].Replace("{size}", "480"));
                           using (IRandomAccessStream iras = b.AsBuffer().AsStream().AsRandomAccessStream())
                           {
                               await wb.SetSourceAsync(iras);
                           }
                           BlurEffect be = new BlurEffect(wb);
                           mainGrid.Background = new ImageBrush() { ImageSource = await be.ApplyFilter(1), Stretch = Stretch.UniformToFill };
                       }
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

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var applicationView = ApplicationView.GetForCurrentView();
                StatusBar statusBar = StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
            }
            DataTransferManager.GetForCurrentView().DataRequested += PlayePage_DataRequested;
        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var applicationView = ApplicationView.GetForCurrentView();
                StatusBar statusBar = StatusBar.GetForCurrentView();
                await statusBar.ShowAsync();
            }
            DataTransferManager.GetForCurrentView().DataRequested -= PlayePage_DataRequested;
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
            nowplay = await Class.Model.Player.GetNowPlay();
            var playlist = await Class.Model.PlayList.GetPlayList();
            if (playlist != null)
            {
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
            }
            NowLrc_Text.Text = "";
            if (nowplay != null)
            {
                SongName_Text.Text = nowplay.title;
                SingerName_Text.Text = nowplay.singername;
                
                if (nowplay.albumid.Length > 0)
                {
                    var imgurl = await GetAlbumImg(nowplay.albumid);
                    Album_Img.Source = new BitmapImage(new Uri(imgurl));
                    WriteableBitmap wb = new WriteableBitmap(1000, 1500);
                    HttpClient hc = new HttpClient();
                    byte[] b = await hc.GetByteArrayAsync(imgurl);
                    using (IRandomAccessStream iras = b.AsBuffer().AsStream().AsRandomAccessStream())
                    {
                        await wb.SetSourceAsync(iras);
                    }
                    BlurEffect be = new BlurEffect(wb);
                    albumimgbrush= new ImageBrush() { ImageSource = await be.ApplyFilter(2), Stretch = Stretch.UniformToFill };
                    mainGrid.Background = new ImageBrush() { ImageSource = await be.ApplyFilter(2), Stretch = Stretch.UniformToFill };
                }
                else
                {
                    Album_Img.Source = new BitmapImage() { UriSource = new Uri("ms-appx:///Assets/image/albumimg2.png") };
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
            var Theme = Application.Current.Resources.MergedDictionaries[0] as ResourceDictionary;
            try
            {
                switch (flipview.SelectedIndex)
                {
                    case 0:
                        Page1_Icon.Fill = new SolidColorBrush(Colors.White);
                        Page2_Icon.Fill = new SolidColorBrush(Color.FromArgb(50, 225, 225, 225));
                        if (albumimgbrush == null)
                        {
                            mainGrid.Background = (ImageBrush)Theme["KuGou-Background"];
                        }else
                        {
                            mainGrid.Background = albumimgbrush;
                        }
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
            try
            {
                if (singerdata == null)
                {
                    var nowplay = await Class.Model.Player.GetNowPlay();
                    singerdata = await Class.Model.SearchResultModel.GetSingerResult(nowplay.singername);
                    if (singerdata != null && singerdata.pics != null && singerdata.pics.Count > 0)
                    {
                        WriteableBitmap wb = new WriteableBitmap(1000, 1500);
                        HttpClient hc = new HttpClient();
                        byte[] b = await hc.GetByteArrayAsync(singerdata.pics[0].Replace("{size}", "480"));
                        using (IRandomAccessStream iras = b.AsBuffer().AsStream().AsRandomAccessStream())
                        {
                            await wb.SetSourceAsync(iras);
                        }
                        BlurEffect be = new BlurEffect(wb);
                        mainGrid.Background = new ImageBrush() { ImageSource = await be.ApplyFilter(2), Stretch = Stretch.UniformToFill };
                        PicTimer.Start();
                    }
                }
                else
                {
                    if (singerdata != null && singerdata.pics != null && singerdata.pics.Count > 0)
                    {
                        WriteableBitmap wb = new WriteableBitmap(1000, 1500);
                        HttpClient hc = new HttpClient();
                        byte[] b = await hc.GetByteArrayAsync(singerdata.pics[picnum].Replace("{size}", "480"));
                        using (IRandomAccessStream iras = b.AsBuffer().AsStream().AsRandomAccessStream())
                        {
                            await wb.SetSourceAsync(iras);
                        }
                        BlurEffect be = new BlurEffect(wb);
                        mainGrid.Background = new ImageBrush() { ImageSource = await be.ApplyFilter(2),Stretch=Stretch.UniformToFill };
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        private void Back_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                Frame.Navigate(typeof(MainPage));
            }
        }

        private void SearchBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Pages.Search));
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
            var httpclient = new System.Net.Http.HttpClient();
            var json= await httpclient.GetStringAsync("http://mobilecdn.kugou.com/api/v3/album/info?albumid=" + Albumid);
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

        private async void PlayePage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            try
            {
                var deferral = args.Request.GetDeferral();
                var httpclient = new System.Net.Http.HttpClient();
                var geturl = string.Format("http://www.kugou.com/clientshare/app/?md5={0}&cmid=5&pid=android&hash={1}&url=http%3A%2F%2Fm.kugou.com%2Fweibo%2F%3Faction%3Dsingle%26filename%3D{2}%26hash%3D{1}", MD5.GetMd5String("kgclientshare" + nowplay.hash), nowplay.hash, nowplay.singername + "-" + nowplay.title);
                var json = await httpclient.GetStringAsync(geturl);
                var url = Windows.Data.Json.JsonObject.Parse(json)["data"].GetString();
                args.Request.Data.SetText("我正在听:" + nowplay.singername + "-" + nowplay.title + "   ---来自酷狗音乐UWP客户端" + Environment.NewLine + url);
                args.Request.Data.Properties.Title = "酷狗音乐UWP";
                args.Request.Data.Properties.Description = "分享音乐";
                deferral.Complete();
            }
            catch (Exception)
            {
                await new Windows.UI.Popups.MessageDialog("分享失败,请检查网络连接").ShowAsync();
            }
        }

        private async void ToolBtnClicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            switch (btn.TabIndex)
            {
                case 0:
                    await new Windows.UI.Popups.MessageDialog("正在建设").ShowAsync();
                    break;
                case 1:
                    if (nowplay != null && nowplay.url.Contains("http://")&&nowplay.hash!=null&&nowplay.hash!="")
                    {
                        DataTransferManager.ShowShareUI();
                    }
                    else
                    {
                        await new Windows.UI.Popups.MessageDialog("本地歌曲无法分享").ShowAsync();
                    }
                    break;
                case 2:
                    LoadProgress.Visibility = Visibility.Visible;
                    nowplay = await Class.Model.Player.GetNowPlay();
                    if (nowplay.url != null && nowplay.url.Contains("http://"))
                    {
                        await KG_ClassLibrary.BackgroundDownload.Start(nowplay.singername + "-" + nowplay.title, nowplay.url, KG_ClassLibrary.BackgroundDownload.DownloadType.song);
                        await new Windows.UI.Popups.MessageDialog(ResourceLoader.GetForCurrentView().GetString("AddDownSuccess")).ShowAsync();
                    }
                    else
                    {
                        await new Windows.UI.Popups.MessageDialog(ResourceLoader.GetForCurrentView().GetString("DownLocalFalied") + nowplay.url).ShowAsync();
                    }
                    LoadProgress.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    if (nowplay != null)
                    {
                        if (nowplay.singername == null || nowplay.singername == "未知歌手" || nowplay.singername == "传播好音乐")
                        {
                            MoreToolBtn2.Visibility = Visibility.Collapsed;
                        }
                        if (singerdata == null || singerdata.pics == null)
                        {
                            MoreToolBtn3.Visibility = Visibility.Collapsed;
                        }
                        MoreToolBtnBar.IsOpen = true;
                    }
                    break;
                default:
                    break;
            }
        }

        private void Album_Img_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var img = sender as Image;
            var source = img.Source as BitmapImage;
            if (source.UriSource.ToString().Contains("http://"))
            {
                Frame.Navigate(typeof(Pages.AlbumPage), nowplay.albumid);
            }
        }

        private async void MoreToolBtnsClicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            switch (btn.TabIndex)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    Frame.Navigate(typeof(Pages.SearchResult), nowplay.singername);
                    break;
                case 3:
                    LoadProgress.Visibility = Visibility.Visible;
                    HttpClient hc = new HttpClient();
                    var bytes = await hc.GetByteArrayAsync(singerdata.pics[picnum].Replace("{size}", "480"));
                    var picfolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("保存的歌手写真", CreationCollisionOption.OpenIfExists);
                    var picfile = await picfolder.CreateFileAsync(nowplay.singername + picnum.ToString(), CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBytesAsync(picfile, bytes);
                    await new Windows.UI.Popups.MessageDialog(ResourceLoader.GetForCurrentView().GetString("DownSingerPic")).ShowAsync();
                    LoadProgress.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
        }
    }
}
