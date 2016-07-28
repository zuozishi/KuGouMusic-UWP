using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 酷狗音乐UWP.Class;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace 酷狗音乐UWP.page
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MVPlayer : Page
    {
        DispatcherTimer displayTimer = new DispatcherTimer();
        private MVData mvdata;

        public MVPlayer()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var dispRequest = new DisplayRequest();
                dispRequest.RequestActive();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
                ApplicationView.GetForCurrentView().ExitFullScreenMode();
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var hash = e.Parameter.ToString();
            mvdata = await get_mv_url(hash);
            displayTimer.Interval = TimeSpan.FromSeconds(10);
            displayTimer.Tick += DisplayTimer_Tick;
            try
            {
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                {
                    var dispRequest = new DisplayRequest();
                    dispRequest.RequestActive();
                    DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
                    ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                }
            }
            catch (Exception)
            {
                
            }
            mediaTransport.DataContext = new mvinfo() { songname = mvdata.songname, singer = mvdata.singer };
            Class.Model.History.Add(new Class.Model.History.HistoryMV() { title= mvdata.songname,singer= mvdata.singer,hash=hash});
            if (mvdata.hasrq)
            {
                media.Source = new Uri(mvdata.rq.downurl);
            }else
            {
                if (mvdata.hassq)
                {
                    media.Source = new Uri(mvdata.sq.downurl);
                }else
                {
                    media.Source = new Uri(mvdata.le.downurl);
                }
            }
        }

        public class mvinfo
        {
            public string songname { get; set; }
            public string singer { get; set; }
        }

        private void DisplayTimer_Tick(object sender, object e)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var dispRequest = new DisplayRequest();
                dispRequest.RequestActive();
            }
        }

        public class MVData
        {
            public class sqmv
            {
                public string hash, downurl;
                public int filesize, timelength;
            }
            public class rqmv
            {
                public string hash, downurl;
                public int filesize, timelength;
            }
            public class remv
            {
                public string hash, downurl;
                public int filesize, timelength;
            }
            public sqmv sq;
            public rqmv rq;
            public remv le;
            public bool hassq
            {
                get
                {
                    if (sq != null && sq.downurl != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            public bool hasrq
            {
                get
                {
                    if (rq != null && rq.downurl != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            public bool hasle
            {
                get
                {
                    if (le != null && le.downurl != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            public string songname, singer;
        }
        public static async Task<MVData> get_mv_url(string hash)
        {
            try
            {
                var sign = MD5.GetMd5String(hash + "kugoumvcloud");
                var request = new Noear.UWP.Http.AsyncHttpClient();
                request.Url("http://trackermv.kugou.com/interface/index?cmd=100&pid=2&ext=mp4&hash=" + hash + "&quality=3&key=" + sign);
                var result = await request.Get();
                var json = result.GetString();
                JsonObject obj = JsonObject.Parse(json);
                obj.GetNamedObject("mvdata").Add("songname", JsonValue.CreateStringValue(obj.GetNamedString("songname")));
                obj.GetNamedObject("mvdata").Add("singer", JsonValue.CreateStringValue(obj.GetNamedString("singer")));
                json = obj.GetNamedObject("mvdata").ToString();
                return data.DataContractJsonDeSerialize<MVData>(json);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void GoBackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void SelecionQuButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            if (mvdata != null)
            {
                var dialog = new MessageDialog("选择播放清晰度");
                if (mvdata.hasle)
                {
                    dialog.Commands.Add(new UICommand("标清", SelectionQuClicked,mvdata.le.downurl));
                    if (media.Source.ToString() == mvdata.le.downurl)
                    {
                        dialog.DefaultCommandIndex = (uint)(dialog.Commands.Count - 1);
                    }
                }
                if (mvdata.hassq)
                {
                    dialog.Commands.Add(new UICommand("高清", SelectionQuClicked, mvdata.sq.downurl));
                    if (media.Source.ToString() == mvdata.sq.downurl)
                    {
                        dialog.DefaultCommandIndex = (uint)(dialog.Commands.Count - 1);
                    }
                }
                if (mvdata.hasrq)
                {
                    dialog.Commands.Add(new UICommand("超清", SelectionQuClicked, mvdata.rq.downurl));
                    if (media.Source.ToString() == mvdata.rq.downurl)
                    {
                        dialog.DefaultCommandIndex = (uint)(dialog.Commands.Count - 1);
                    }
                }
                
                await dialog.ShowAsync();
            }
        }

        private void SelectionQuClicked(IUICommand command)
        {
            media.Source = new Uri(command.Id.ToString());
        }

        private async void SelectionDownClicked(IUICommand command)
        {
            await KG_ClassLibrary.BackgroundDownload.Start(mvdata.singer + "-" + mvdata.songname, command.Id.ToString(), KG_ClassLibrary.BackgroundDownload.DownloadType.mv);
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            if (mvdata != null)
            {
                var dialog = new MessageDialog("选择下载清晰度");
                if (mvdata.hasle)
                {
                    dialog.Commands.Add(new UICommand("标清", SelectionDownClicked, mvdata.le.downurl));
                    if (media.Source.ToString() == mvdata.le.downurl)
                    {
                        dialog.DefaultCommandIndex = (uint)(dialog.Commands.Count - 1);
                    }
                }
                if (mvdata.hassq)
                {
                    dialog.Commands.Add(new UICommand("高清", SelectionDownClicked, mvdata.sq.downurl));
                    if (media.Source.ToString() == mvdata.sq.downurl)
                    {
                        dialog.DefaultCommandIndex = (uint)(dialog.Commands.Count - 1);
                    }
                }
                if (mvdata.hasrq)
                {
                    dialog.Commands.Add(new UICommand("超清", SelectionDownClicked, mvdata.rq.downurl));
                    if (media.Source.ToString() == mvdata.rq.downurl)
                    {
                        dialog.DefaultCommandIndex = (uint)(dialog.Commands.Count - 1);
                    }
                }

                await dialog.ShowAsync();
            }
        }
    }
}
