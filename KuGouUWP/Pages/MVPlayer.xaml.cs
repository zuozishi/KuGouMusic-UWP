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
using KuGouUWP.Class;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace KuGouUWP.Pages
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
                var request = new System.Net.Http.HttpClient();
                var json =await request.GetStringAsync("http://trackermv.kugou.com/interface/index?cmd=100&pid=2&ext=mp4&hash=" + hash + "&quality=3&key=" + sign);
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

        private void SelecionQuButton_Click(object sender, RoutedEventArgs e)
        {
            if (mvdata != null)
            {
                QuPanel.Tag = "Qu";
                var lebtn = QuBtnPanel.Children[0] as Button;
                var sqbtn = QuBtnPanel.Children[1] as Button;
                var rqbtn = QuBtnPanel.Children[2] as Button;
                if (mvdata.hasle&&media.Source.ToString()!= mvdata.le.downurl)
                {
                    lebtn.Tag = mvdata.le.downurl;
                    lebtn.Visibility = Visibility.Visible;
                }
                if (mvdata.hassq && media.Source.ToString() != mvdata.sq.downurl)
                {
                    sqbtn.Tag = mvdata.sq.downurl;
                    sqbtn.Visibility = Visibility.Visible;
                }
                if (mvdata.hasrq && media.Source.ToString() != mvdata.rq.downurl)
                {
                    rqbtn.Tag = mvdata.rq.downurl;
                    rqbtn.Visibility = Visibility.Visible;
                }
                QuPanel.Visibility = Visibility.Visible;
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (mvdata != null)
            {
                QuPanel.Tag = "Down";
                var lebtn = QuBtnPanel.Children[0] as Button;
                var sqbtn = QuBtnPanel.Children[1] as Button;
                var rqbtn = QuBtnPanel.Children[2] as Button;
                if (mvdata.hasle)
                {
                    lebtn.Tag = mvdata.le.downurl;
                    lebtn.Visibility = Visibility.Visible;
                }
                if (mvdata.hassq)
                {
                    sqbtn.Tag = mvdata.sq.downurl;
                    sqbtn.Visibility = Visibility.Visible;
                }
                if (mvdata.hasrq)
                {
                    rqbtn.Tag = mvdata.rq.downurl;
                    rqbtn.Visibility = Visibility.Visible;
                }
                QuPanel.Visibility = Visibility.Visible;
            }
        }

        private void HidenQuPanel(object sender, RoutedEventArgs e)
        {
            QuPanel.Tag = "";
            var lebtn = QuBtnPanel.Children[0] as Button;
            var sqbtn = QuBtnPanel.Children[1] as Button;
            var rqbtn = QuBtnPanel.Children[2] as Button;
            lebtn.Visibility = Visibility.Collapsed;
            sqbtn.Visibility = Visibility.Collapsed;
            rqbtn.Visibility = Visibility.Collapsed;
            QuPanel.Visibility = Visibility.Collapsed;
        }

        private async void QuBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (QuPanel.Tag != null && QuPanel.Tag.ToString() == "Qu")
            {
                media.Source = new Uri(btn.Tag.ToString());
            }
            if (QuPanel.Tag != null && QuPanel.Tag.ToString() == "Down")
            {
                await KG_ClassLibrary.BackgroundDownload.Start(mvdata.singer + "-" + mvdata.songname, btn.Tag.ToString(), KG_ClassLibrary.BackgroundDownload.DownloadType.mv);
            }
            QuPanel.Tag = "";
            var lebtn = QuBtnPanel.Children[0] as Button;
            var sqbtn = QuBtnPanel.Children[1] as Button;
            var rqbtn = QuBtnPanel.Children[2] as Button;
            lebtn.Visibility = Visibility.Collapsed;
            sqbtn.Visibility = Visibility.Collapsed;
            rqbtn.Visibility = Visibility.Collapsed;
            QuPanel.Visibility = Visibility.Collapsed;
        }
    }
}
