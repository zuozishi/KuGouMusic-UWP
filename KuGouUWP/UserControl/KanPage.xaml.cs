using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace KuGouUWP.UserControlClass
{
    public sealed partial class KanPage : UserControl
    {
        private List<BannerData> bannerdata;
        Frame mainFrame = Window.Current.Content as Frame;

        public KanPage()
        {
            this.InitializeComponent();
        }
        public async void LoadData()
        {
            LoadProcess.IsActive = true;
            await LoadBannerData();
            await LoadMVData();
            LoadProcess.IsActive = false;
        }

        private async Task LoadMVData()
        {
            var mvdata = await MVDataInGroup.GetData();
            if (mvdata != null)
            {
                this.mvitemcollectSource.Source = mvdata;
                ZoomOutView.ItemsSource = mvitemcollectSource.View.CollectionGroups;
                ZoomInView.ItemsSource = mvitemcollectSource.View;
                ZoomInView.SelectionMode = ListViewSelectionMode.Extended;
                ZoomInView.SelectionChanged += ZoomInView_SelectionChanged;
            }
        }

        public async Task LoadBannerData()
        {
            bannerdata = await BannerData.GetBanner();
            if (bannerdata != null)
            {
                var piclist = new List<string>();
                foreach (var item in bannerdata)
                {
                    piclist.Add(item.bannerurl);
                }
                picview.SetItems(piclist);
                picview.picflipview.Tapped += Picflipview_Tapped;
            }
        }

        private void Picflipview_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var view = sender as FlipView;
            if (view.SelectedIndex >= 0)
            {
                switch (bannerdata[view.SelectedIndex].type)
                {
                    case 1:
                        mainFrame.Navigate(typeof(Pages.MVPlayer), bannerdata[view.SelectedIndex].extra.hash);
                        break;
                    case 2:
                        mainFrame.Navigate(typeof(Pages.SongListPage), new string[] { bannerdata[view.SelectedIndex].extra.specialid, bannerdata[view.SelectedIndex].extra.title });
                        break;
                    default:
                        break;
                }
            }
        }

        public class BannerData
        {
            public int type { get; set; }
            public string bannerurl { get; set; }
            public Extra extra { get; set; }
            public class Extra
            {
                public string hash { get; set; }
                public string specialid { get; set; }
                public string title { get; set; }
                public string url { get; set; }
            }
            public static async Task<List<BannerData>> GetBanner()
            {
                try
                {
                    var httpclient = new System.Net.Http.HttpClient();
                    var json = await httpclient.GetStringAsync("http://mobilecdn.kugou.com/api/v3/mv/multBanner?plat=0&version=8150");
                    var obj = Windows.Data.Json.JsonObject.Parse(json);
                    var data = Class.data.DataContractJsonDeSerialize<List<BannerData>>(obj.GetNamedObject("data").GetNamedArray("info").ToString());
                    return data;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public class MVDataInGroup
        {
            public string key { get; set; }
            public List<MVData> MVContent { get; set; }
            public static async Task<List<MVDataInGroup>> GetData()
            {
                try
                {
                    var httpclient = new System.Net.Http.HttpClient();
                    var json = await httpclient.GetStringAsync("http://mobilecdn.kugou.com/api/v3/mv/recommend?plat=0&version=8150");
                    json = json.Replace("{size}", "400");
                    var obj = Windows.Data.Json.JsonObject.Parse(json);
                    obj = obj.GetNamedObject("data");
                    var resultgroup = new List<MVDataInGroup>();
                    var tempmvlist = new List<MVData>();
                    var tempmvobj = obj.GetNamedArray("new");
                    for (int i = 0; i < tempmvobj.Count; i++)
                    {
                        tempmvobj[i].GetObject().Add("Content", JsonValue.CreateStringValue("MV首播"));
                    }
                    tempmvlist = Class.data.DataContractJsonDeSerialize<List<MVData>>(tempmvobj.ToString());
                    resultgroup.Add(new MVDataInGroup() { key = "MV首播", MVContent = tempmvlist });

                    tempmvobj = obj.GetNamedArray("hot");
                    for (int i = 0; i < tempmvobj.Count; i++)
                    {
                        tempmvobj[i].GetObject().Add("Content", JsonValue.CreateStringValue("最热MV"));
                    }
                    tempmvlist = Class.data.DataContractJsonDeSerialize<List<MVData>>(tempmvobj.ToString());
                    resultgroup.Add(new MVDataInGroup() { key = "最热MV", MVContent = tempmvlist });

                    tempmvobj = obj.GetNamedArray("live");
                    for (int i = 0; i < tempmvobj.Count; i++)
                    {
                        tempmvobj[i].GetObject().Add("Content", JsonValue.CreateStringValue("现场"));
                    }
                    tempmvlist = Class.data.DataContractJsonDeSerialize<List<MVData>>(tempmvobj.ToString());
                    resultgroup.Add(new MVDataInGroup() { key = "现场", MVContent = tempmvlist });

                    tempmvobj = obj.GetNamedArray("topic");
                    for (int i = 0; i < tempmvobj.Count; i++)
                    {
                        for (int j = 0; j < tempmvobj[i].GetObject().GetNamedArray("mvs").GetArray().Count; j++)
                        {
                            tempmvobj[i].GetObject().GetNamedArray("mvs").GetArray()[j].GetObject().Add("Content", JsonValue.CreateStringValue(tempmvobj[i].GetObject().GetNamedString("title")));
                        }
                        tempmvlist = Class.data.DataContractJsonDeSerialize<List<MVData>>(tempmvobj[i].GetObject().GetNamedArray("mvs").ToString());
                        resultgroup.Add(new MVDataInGroup() { key = tempmvobj[i].GetObject().GetNamedString("title"), MVContent = tempmvlist });
                    }

                    tempmvobj = obj.GetNamedArray("mainland");
                    for (int i = 0; i < tempmvobj.Count; i++)
                    {
                        tempmvobj[i].GetObject().Add("Content", JsonValue.CreateStringValue("内地"));
                    }
                    tempmvlist = Class.data.DataContractJsonDeSerialize<List<MVData>>(tempmvobj.ToString());
                    resultgroup.Add(new MVDataInGroup() { key = "内地", MVContent = tempmvlist });

                    tempmvobj = obj.GetNamedArray("hktw");
                    for (int i = 0; i < tempmvobj.Count; i++)
                    {
                        tempmvobj[i].GetObject().Add("Content", JsonValue.CreateStringValue("港台"));
                    }
                    tempmvlist = Class.data.DataContractJsonDeSerialize<List<MVData>>(tempmvobj.ToString());
                    resultgroup.Add(new MVDataInGroup() { key = "港台", MVContent = tempmvlist });

                    tempmvobj = obj.GetNamedArray("korea");
                    for (int i = 0; i < tempmvobj.Count; i++)
                    {
                        tempmvobj[i].GetObject().Add("Content", JsonValue.CreateStringValue("韩国"));
                    }
                    tempmvlist = Class.data.DataContractJsonDeSerialize<List<MVData>>(tempmvobj.ToString());
                    resultgroup.Add(new MVDataInGroup() { key = "韩国", MVContent = tempmvlist });

                    tempmvobj = obj.GetNamedArray("japan");
                    for (int i = 0; i < tempmvobj.Count; i++)
                    {
                        tempmvobj[i].GetObject().Add("Content", JsonValue.CreateStringValue("日本"));
                    }
                    tempmvlist = Class.data.DataContractJsonDeSerialize<List<MVData>>(tempmvobj.ToString());
                    resultgroup.Add(new MVDataInGroup() { key = "日本", MVContent = tempmvlist });

                    tempmvobj = obj.GetNamedArray("west");
                    for (int i = 0; i < tempmvobj.Count; i++)
                    {
                        tempmvobj[i].GetObject().Add("Content", JsonValue.CreateStringValue("欧美"));
                    }
                    tempmvlist = Class.data.DataContractJsonDeSerialize<List<MVData>>(tempmvobj.ToString());
                    resultgroup.Add(new MVDataInGroup() { key = "欧美", MVContent = tempmvlist });
                    return resultgroup;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        public class MVData
        {
            public string filename { get; set; }
            public string singername { get; set; }
            public string hash { get; set; }
            public string Content { get; set; }
            public string imgurl { get; set; }
        }

        private void ZoomInView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedItem != null)
            {
                var data = list.SelectedItem as MVData;
                mainFrame.Navigate(typeof(Pages.MVPlayer), data.hash);
            }
        }

        public delegate void TuiJianBtnHandler(string ids);
        public event TuiJianBtnHandler TuiJianTopBtnCLicked;
        private void TuiJianTopBtn_CLicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (TuiJianTopBtnCLicked == null)
            {
                mainFrame.Navigate(typeof(Pages.YueKu.MVPage), (string)btn.Tag);
            }else
            {
                TuiJianTopBtnCLicked((string)btn.Tag);
            }
        }
    }
}
