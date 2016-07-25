using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace 酷狗音乐UWP.page.YueKu
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SingerListPage : Page
    {
        public SingerListPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            var data = e.Parameter as string[];
            string type = data[0];
            string sextype = data[1];
            TitleText.Text = data[2];
            LoadProcess.IsActive = true;
            var singerdata= await SingerData.LoadData(type, sextype);
            if (singerdata != null && singerdata.Count > 0)
            {
                if (singerdata[0].title == "热门")
                {
                    HotSingerBtn1.DataContext = singerdata[0].singer[0];
                    HotSingerBtn2.DataContext = singerdata[0].singer[1];
                    HotSingerBtn3.DataContext = singerdata[0].singer[2];
                    HotSingerBtn4.DataContext = singerdata[0].singer[3];
                    HotSingerBtn5.DataContext = singerdata[0].singer[4];
                    HotSingerBtn6.DataContext = singerdata[0].singer[5];
                    HotSingerBtn7.DataContext = singerdata[0].singer[6];
                    HotSingerBtn8.DataContext = singerdata[0].singer[7];
                    singerdata.RemoveAt(0);
                }
            }
            var singerlist = new List<SingerInGroup>();
            foreach (var group in singerdata)
            {
                var list = new SingerInGroup();
                list.Key = group.title;
                list.ItemContent = group.singer;
                singerlist.Add(list);
            }
            this.itemcollectSource.Source = singerlist;
            ZoomOutView.ItemsSource = itemcollectSource.View.CollectionGroups;
            ZoomInView.ItemsSource = itemcollectSource.View;
            LoadProcess.IsActive = false;
        }

        public class SingerInGroup
        {
            public string Key { get; set; }
            public List<SingerList> ItemContent { get; set; }
        }

        public class SingerList
        {
            public string singername { get; set; }
            public string singerid { get; set; }
            public string imgurl { get; set; }
            public string Content { get; set; }
        }

        public class SingerData
        {
            public List<SingerList> singer;
            public string title { get; set; }
            public static async Task<List<SingerData>> LoadData(string type,string sextype)
            {
                var httpclient = new Noear.UWP.Http.AsyncHttpClient();
                httpclient.Url("http://mobilecdn.kugou.com/api/v3/singer/list?type="+type+"&showtype=2&sextype="+sextype+"&musician=0");
                var json = (await httpclient.Get()).GetString();
                json = json.Replace("{size}", "150");
                var obj = Windows.Data.Json.JsonObject.Parse(json);
                var data= Class.data.DataContractJsonDeSerialize<List<SingerData>>(obj.GetNamedObject("data").GetNamedArray("info").ToString());
                for (int i = 0; i < data.Count; i++)
                {
                    for (int j = 0; j < data[i].singer.Count; j++)
                    {
                        data[i].singer[j].Content = data[i].title;
                    }
                }
                return data;
            }
        }

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void HotSingerBtnClicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.DataContext != null)
            {
                var data = btn.DataContext as SingerList;
                Frame.Navigate(typeof(page.SingerPage), data.singerid);
            }
        }
    }
}
