using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace 酷狗音乐UWP.page
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AlbumPage : Page
    {
        public AlbumPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var albumid = e.Parameter.ToString();
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var applicationView = ApplicationView.GetForCurrentView();
                applicationView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = Colors.White;
                statusBar.BackgroundColor = Color.FromArgb(1, 68, 190, 239);
                statusBar.BackgroundOpacity = 0;
            }
            albuminfo_Grid.DataContext = await GetAlbumInfo(albumid);
            SongList.ItemsSource = await GetSongList(albumid);
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
            }
        }

        private async Task<Album_Info> GetAlbumInfo(string albumid)
        {
            var httpclient = new Noear.UWP.Http.AsyncHttpClient();
            httpclient.Url("http://mobilecdn.kugou.com/api/v3/album/info?albumid="+albumid);
            var json = (await httpclient.Get()).GetString();
            json = json.Replace("{size}", "400");
            var obj = Windows.Data.Json.JsonObject.Parse(json);
            var data = Class.data.DataContractJsonDeSerialize<Album_Info>(obj.GetNamedObject("data").ToString());
            data.publishtime = data.publishtime.Replace("00:00:00", "");
            data.publishtime = data.publishtime + "发行";
            GetSingerInfo(data.singerid);
            return data;
        }

        public async Task<List<SongData>> GetSongList(string albumid)
        {
            var httpclient = new Noear.UWP.Http.AsyncHttpClient();
            httpclient.Url("http://mobilecdn.kugou.com/api/v3/album/song?pagesize=-1&plat=0&page=1&version=8150&albumid=" + albumid);
            var json = (await httpclient.Get()).GetString();
            json = json.Replace("320hash", "hash320");
            var obj = Windows.Data.Json.JsonObject.Parse(json);
            var data = Class.data.DataContractJsonDeSerialize<List<SongData>>(obj.GetNamedObject("data").GetNamedArray("info").ToString());
            foreach (var item in data)
            {
                item.filename = item.filename.Replace(" ", "");
                item.title = item.filename.Split('-')[1];
                item.singername = item.filename.Split('-')[0];
                if(item.mvhash=="")
                {
                    item.hasmv = "Collapsed";
                }
                else
                {
                    item.hasmv = "Visible";
                }
            }
            return data;
        }

        public async void GetSingerInfo(string singerid)
        {
            var httpclient = new Noear.UWP.Http.AsyncHttpClient();
            httpclient.Url("http://mobilecdn.kugou.com/api/v3/singer/info?singerid=" + singerid);
            var json = (await httpclient.Get()).GetString();
            json = json.Replace("{size}", "150");
            var obj = Windows.Data.Json.JsonObject.Parse(json);
            singerimg.ImageSource = new Windows.UI.Xaml.Media.Imaging.BitmapImage() { UriSource = new Uri(obj.GetNamedObject("data").GetNamedString("imgurl")) };
        }

        public class Album_Info
        {
            public string albumname { get; set; }
            public string singerid { get; set; }
            public string singername { get; set; }
            public string intro { get; set; }
            public string imgurl { get; set; }
            public string publishtime { get; set; }
        }

        public class SongData
        {
            public string filename { get; set; }
            public string title { get; set; }
            public string singername { get; set; }
            public string hash { get; set; }
            public string mvhash { get; set; }
            public string hasmv { get; set; }
            public string sqhash { get; set; }
            public string hash320 { get; set; }
        }

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
