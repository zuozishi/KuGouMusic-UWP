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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace KuGouUWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MVListPage : Page
    {
        public MVListPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                var vlistid = e.Parameter.ToString();
                var httpclient = new System.Net.Http.HttpClient();
                var json = await httpclient.GetStringAsync("http://service.mobile.kugou.com/v1/mv/vspecialdetail?vid=" + vlistid);
                json = json.Replace("{size}", "240");
                var obj = Windows.Data.Json.JsonObject.Parse(json);
                var vlistdata = Class.data.DataContractJsonDeSerialize<MVListData>(obj.GetNamedObject("data").GetNamedObject("info").ToString());
                mainGrid.DataContext = vlistdata;
            }
            catch (Exception)
            {
                
            }
        }

        public class MVListData
        {
            public string des { get; set; }
            public string mobile_banner { get; set; }
            public string mvbanner { get; set; }
            public string hot_num { get; set; }
            public string public_time { get; set; }
            public string title { get; set; }
            public List<MVList> mvlist { get; set; }
            public class MVList
            {
                public string name { get; set; }
                public string singer { get; set; }
                public string pic { get; set; }
                public string mv_hash { get; set; }
            }
        }

        private void mvlistView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as MVListData.MVList;
            Frame.Navigate(typeof(Pages.MVPlayer), data.mv_hash);
        }

        private void mvlistView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            var data = list.SelectedItem as MVListData.MVList;
            Frame.Navigate(typeof(Pages.MVPlayer), data.mv_hash);
        }
    }
}
