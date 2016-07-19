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
            var albuminfo = await GetAlbumInfo(albumid);
            albuminfo_Grid.DataContext = albuminfo;
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
            return data;
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

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
