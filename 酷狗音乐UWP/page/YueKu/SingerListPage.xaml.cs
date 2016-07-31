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
        private ObservableCollection<SingerInGroup> singerlist;
        private ObservableCollection<SingerInGroup> newgroup;
        private ObservableCollection<SingerData> singerdata;

        public SingerListPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            ZoomInView.SelectedIndex = -1;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (singerdata == null)
            {
                var data = e.Parameter as string[];
                string type = data[0];
                string sextype = data[1];
                TitleText.Text = data[2];
                LoadProcess.IsActive = true;
                singerdata = await SingerData.LoadData(type, sextype);
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
                singerlist = new ObservableCollection<SingerInGroup>();
                foreach (var group in singerdata)
                {
                    var list = new SingerInGroup();
                    list.Key = group.title;
                    list.ItemContent = group.singer;
                    singerlist.Add(list);
                }
                this.itemcollectSource.Source = singerlist;
                newgroup = new ObservableCollection<SingerInGroup>();
                newgroup.Add(singerlist[0]);
                this.singeritemcollectSource.Source = newgroup;
                ZoomOutView.ItemsSource = itemcollectSource.View.CollectionGroups;
                ZoomInView.ItemsSource = singeritemcollectSource.View;
                ZoomInView.SelectionMode = ListViewSelectionMode.Extended;
                ZoomInView.SelectionChanged += ZoomInView_SelectionChanged;
                ZoomInView.SelectedIndex = -1;
                LoadProcess.IsActive = false;
            }
        }

        private void ZoomInView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedItem != null)
            {
                var data = list.SelectedItem as SingerList;
                Frame.Navigate(typeof(page.SingerPage), data.singerid);
                list.SelectedIndex = -1;
            }
        }

        public class SingerInGroup
        {
            public string Key { get; set; }
            public ObservableCollection<SingerList> ItemContent { get; set; }
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
            public ObservableCollection<SingerList> singer;
            public string title { get; set; }
            public static async Task<ObservableCollection<SingerData>> LoadData(string type,string sextype)
            {
                var httpclient = new Noear.UWP.Http.AsyncHttpClient();
                httpclient.Url("http://mobilecdn.kugou.com/api/v3/singer/list?type="+type+"&showtype=2&sextype="+sextype+"&musician=0");
                var json = (await httpclient.Get()).GetString();
                json = json.Replace("{size}", "150");
                var obj = Windows.Data.Json.JsonObject.Parse(json);
                var data= Class.data.DataContractJsonDeSerialize<ObservableCollection<SingerData>>(obj.GetNamedObject("data").GetNamedArray("info").ToString());
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

        private void ZoomOutViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var border = sender as Border;
            var data = border.DataContext as string;
            foreach (var item in singerlist)
            {
                if (item.Key == data)
                {
                    newgroup.Clear();
                    newgroup.Add(item);
                    this.singeritemcollectSource.Source = newgroup;
                    ZoomInView.ItemsSource = singeritemcollectSource.View;
                }
            }
        }
    }
}
