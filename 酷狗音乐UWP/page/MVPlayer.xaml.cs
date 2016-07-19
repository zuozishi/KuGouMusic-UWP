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
        public MVPlayer()
        {
            this.InitializeComponent();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var hash = e.Parameter.ToString();
            var data = await get_mv_url(hash);
            try
            {
                var dispRequest = new DisplayRequest();
                dispRequest.RequestActive();
                //DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                //media.IsFullWindow = true;
            }
            catch (Exception)
            {
                
            }
            songname_text.Text = data.songname;
            singername_text.Text = data.singer;
            if(data.sq.downurl==null)
            {
                if (data.rq.downurl == null)
                {
                    media.Source = new Uri(data.le.downurl);
                }
                else
                {
                    media.Source = new Uri(data.rq.downurl);
                }
            }
            else
            {
                media.Source = new Uri(data.sq.downurl);
            }
            LoadAboutData(hash);
            LoadfxData(data.songname);
        }

        private void LoadfxData(string title)
        {
            
        }

        private async void LoadAboutData(string hash)
        {
            var httpclient = new Windows.Web.Http.HttpClient();
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var time = Convert.ToInt64(ts.TotalSeconds).ToString();
            var key = Class.MD5.GetMd5String("1005Ilwieks28dk2k092lksi2UIkp8150" + time);
            var postobj = new JsonObject();
            postobj.Add("appid", JsonValue.CreateStringValue("1005"));
            postobj.Add("mid", JsonValue.CreateStringValue(""));
            postobj.Add("clienttime", JsonValue.CreateStringValue("1468832378"));
            postobj.Add("key", JsonValue.CreateStringValue("15b07bf1cbe69ed15ce31d667399477d"));
            postobj.Add("clientver", JsonValue.CreateStringValue("8150"));
            var array = new JsonArray();
            var videodata = new JsonObject();
            videodata.Add("video_hash", JsonValue.CreateStringValue(hash));
            videodata.Add("video_id", JsonValue.CreateNumberValue(0));
            array.Add(videodata);
            postobj.Add("data", array);
            var postdata = new Windows.Web.Http.HttpStringContent(postobj.ToString());
            var result= await httpclient.PostAsync(new Uri("http://kmr.service.kugou.com/v1/video/related"), postdata);
            var json = await result.Content.ReadAsStringAsync();
            json = json.Replace("{size}", "150");
            var obj = JsonObject.Parse(json);
            AboutMVListView.ItemsSource = Class.data.DataContractJsonDeSerialize<List<AboutMVdata>>(obj.GetNamedArray("data")[0].GetArray().ToString());
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ApplicationView.GetForCurrentView().ExitFullScreenMode();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.None;
        }
        public class mvdata
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
            public string songname, singer;
        }
        public class AboutMVdata
        {
            public string video_name { get; set; }
            public string author_name { get; set; }
            public string intro { get; set; }
            public string thumb { get; set; }
            public string sd_hash { get; set; }
            public string mkv_sd_hash { get; set; }
            public string qhd_hash { get; set; }
            public string ld_hash { get; set; }
        }
        public static async Task<mvdata> get_mv_url(string hash)
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
                return data.DataContractJsonDeSerialize<mvdata>(json);
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

        private void TopBtnClicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            flipview.SelectedIndex = btn.TabIndex;
        }

        private void flipview_Changed(object sender, SelectionChangedEventArgs e)
        {
            var view = sender as FlipView;
            switch (view.SelectedIndex)
            {
                case 0:
                    AboutMV_Btn.BorderBrush= new SolidColorBrush(Colors.White);
                    fxMVList_Btn.BorderBrush = new SolidColorBrush(Colors.Transparent);
                    break;
                case 1:
                    AboutMV_Btn.BorderBrush = new SolidColorBrush(Colors.Transparent);
                    fxMVList_Btn.BorderBrush = new SolidColorBrush(Colors.White);
                    break;
                default:
                    break;
            }
        }
    }
}
