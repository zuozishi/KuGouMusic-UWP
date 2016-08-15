using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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
    public sealed partial class MyMusicListPage : Page
    {
        private string uid=Class.UserManager.GetData(Class.UserManager.Type.uid);
        private List<MuiscListData> listdata;
        private List<RidListData> riddata;

        public MyMusicListPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (listdata == null)
            {
                GetCloudList();
            }
        }

        private async void GetCloudList()
        {
            LoadProgress.IsActive = true;
            var rid= await GetridList();
            if(rid!=null&&rid!="")
            {
                var httpclient = new System.Net.Http.HttpClient();
                var key = Class.MD5.GetMd5String(uid+rid+"18150kgyzone");
                var json= await httpclient.GetStringAsync("http://y.service.kugou.com/song/info?uid="+uid+"&pid=1&mid=&ver=8150&rid="+rid+"&key="+key);
                json = json.Replace("{size}", "150");
                var obj = Windows.Data.Json.JsonObject.Parse(json);
                listdata = Class.data.DataContractJsonDeSerialize<List<MuiscListData>>(obj.GetNamedArray("list").ToString());
                var mylist = new List<MuiscListData>();
                var cloudlist = new List<MuiscListData>();
                for (int i = 0; i < listdata.Count; i++)
                {
                    listdata[i].moredata = riddata[i];
                    if (listdata[i].icon=="")
                    {
                        listdata[i].icon = "ms-appx:///Assets/image/songimg.png";
                    }
                    if (listdata[i].type == "1")
                    {
                        mylist.Add(listdata[i]);
                    }
                    else
                    {
                        cloudlist.Add(listdata[i]);
                    }
                }
                MyMusicList.ItemsSource = mylist;
                CloudMusicList.ItemsSource = cloudlist;
                MyListNum.Text = mylist.Count.ToString();
                CloudListNum.Text = cloudlist.Count.ToString();
                MyMusicList.SelectionChanged += MusicListChanged;
                CloudMusicList.SelectionChanged += MusicListChanged;
                LoadProgress.IsActive = false;
            }
        }

        private void MusicListChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            var item = list.SelectedItem as MuiscListData;
            if (item != null)
            {
                var data = new string[3];
                data[0] = uid;
                data[1] = item.rid;
                data[2] = item.title;
                Frame.Navigate(typeof(Pages.MyMusicListInfo), data);
                list.SelectedIndex = -1;
            }
        }

        private async Task<string> GetridList()
        {
            try
            {
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                var time = Convert.ToInt64(ts.TotalSeconds).ToString();
                var key = Class.MD5.GetMd5String("100010TKbNapLfhKd89VxM" + uid + time + "netfavorite");
                var httpclient = new System.Net.Http.HttpClient();
                var json = await httpclient.GetStringAsync("http://wp.cloudlist.service.kugou.com/song/getlmwithtype?uid=" + uid + "&appid=100010&ts=" + time + "&key=" + key);
                var obj = Windows.Data.Json.JsonObject.Parse(json);
                riddata = Class.data.DataContractJsonDeSerialize<List<RidListData>>(obj.GetNamedArray("data").ToString());
                string result = "";
                for (int i = 0; i < riddata.Count; i++)
                {
                    if (i == riddata.Count - 1)
                    {
                        result = result + riddata[i].listid;
                    }
                    else
                    {
                        result = result + riddata[i].listid + ",";
                    }
                }
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
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

        public class RidListData
        {
            public string listid { get; set; }
            public string listfilecount { get; set; }
            public string listname { get; set; }
        }
        public class MuiscListData
        {
            public RidListData moredata { get; set; }
            public string rid { get; set; }
            public string title { get; set; }
            public string type { get; set; }
            public string icon { get; set; }
        }
    }
}
