using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class YueKuPage : Page
    {
        public TuiJianData alldata { get; private set; }
        public List<ViewMode.PaiHang> paihangdata { get; private set; }
        public ObservableCollection<GeDanData> GeDanListData { get; private set; }
        string gd_sort = "3", gd_page = "1";
        private List<BannerData> bannerdata;

        public YueKuPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        public async Task LoadBannerData()
        {
            var httpclient = new Noear.UWP.Http.AsyncHttpClient();
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var time= Convert.ToInt64(ts.TotalSeconds).ToString();
            var key = Class.MD5.GetMd5String("1100Ilwieks28dk2k092lksi2UIkp8150"+time);
            httpclient.Url("http://ads.service.kugou.com/v1/mobile_fmbanner?userid=366079534&appid=1100&type=4&networktype=1&operator=7&version=8150&plat=0&ismonthly=0&clienttime="+time+"&clientver=8150&mid=&key="+key+"&isvip=0");
            var json = (await httpclient.Get()).GetString();
            json = json.Replace("{size}", "400");
            var obj = Windows.Data.Json.JsonObject.Parse(json);
            bannerdata = Class.data.DataContractJsonDeSerialize<List<BannerData>>(obj.GetNamedObject("data").GetNamedArray("info").ToString());
            var pics = new List<string>();
            foreach (var item in bannerdata)
            {
                pics.Add(item.imgurl);
            }
            bannerView.SetItems(pics);
        }

        public async void LoadTuiJianData()
        {
            TuiJianLoadProgress.IsActive = true;
            //获取推荐页全部资源
            await LoadBannerData();
            var httpclient = new Noear.UWP.Http.AsyncHttpClient();
            httpclient.Url("http://service.mobile.kugou.com/v1/yueku/recommend?type=7&operator=7&plat=0&version=8150");
            var json = (await httpclient.Get()).GetString();
            json = json.Replace("{size}", "150");
            var obj = Windows.Data.Json.JsonObject.Parse(json);
            alldata = Class.data.DataContractJsonDeSerialize<TuiJianData>((obj.GetNamedObject("data").GetNamedObject("info").ToString()));
            //加载最新音乐
            NewSong1_Text.Text = alldata.song[0].filename;
            NewSong1_Img.Source = new BitmapImage() { UriSource=new Uri(alldata.song[0].singerimgurl) };
            NewSong2_Text.Text = alldata.song[1].filename;
            NewSong3_Text.Text = alldata.song[2].filename;
            NewAlbum1_Img.Source = new BitmapImage() { UriSource = new Uri(alldata.album[0].imgurl) };
            NewAlbum1_Text.Text = alldata.album[0].singername + " - 《"+ alldata.album[0].albumname + "》";
            NewAlbum2_Text.Text = alldata.album[1].singername + " - 《" + alldata.album[1].albumname + "》";
            NewAlbum3_Text.Text = alldata.album[2].singername + " - 《" + alldata.album[2].albumname + "》";
            //加载热门歌单
            recommend1.DataContext = alldata.recommend[0].extra;
            recommend2.DataContext = alldata.recommend[1].extra;
            recommend3.DataContext = alldata.recommend[2].extra;
            recommend4.DataContext = alldata.recommend[3].extra;
            recommend5.DataContext = alldata.recommend[4].extra;
            recommend6.DataContext = alldata.recommend[5].extra;
            //加载主题歌单
            specialTitle.Text = alldata.custom_special[0].title;
            specialView1.DataContext = alldata.custom_special[0].special[0];
            specialView2.DataContext = alldata.custom_special[0].special[1];
            specialView3.DataContext = alldata.custom_special[0].special[2];
            //加载MV歌单
            vlist1.DataContext = alldata.vlist[0];
            vlist2.DataContext = alldata.vlist[1];
            vlist3.DataContext = alldata.vlist[2];
            vlist4.DataContext = alldata.vlist[3];
            //加载精选专题
            foreach (var item in alldata.topic)
            {
                topicList.Items.Add(item);
            }
            topicList.SelectionChanged += TopicList_SelectionChanged;
            //加载完成
            TuiJianLoadProgress.IsActive = false;
        }

        public async void LoadBaiHangData()
        {
            PaiHangLoadProgress.IsActive = true;
            //获取排行榜全部资源
            var httpclient = new Noear.UWP.Http.AsyncHttpClient();
            httpclient.Url("http://mobilecdn.kugou.com/api/v3/rank/list?parentid=0&withsong=1&plat=0&apiver=2&showtype=2&version=8150");
            var json = (await httpclient.Get()).GetString();
            json = json.Replace("{size}", "150");
            var obj = Windows.Data.Json.JsonObject.Parse(json);
            var alldata1 = Class.data.DataContractJsonDeSerialize<List<PaiHangData>>((obj.GetNamedObject("data").GetNamedArray("info").ToString()));
            alldata1.RemoveAt(0);
            //加载数据
            paihangdata = new List<ViewMode.PaiHang>();
            foreach (var item in alldata1)
            {
                var paihang = new ViewMode.PaiHang();
                paihang.id = item.id.ToString();
                paihang.rankname = item.rankname;
                paihang.rankid = item.rankid.ToString();
                paihang.imgurl = item.imgurl;
                paihang.song1 = item.songinfo[0].songname;
                paihang.song2 = item.songinfo[1].songname;
                paihang.song3 = item.songinfo[2].songname;
                paihangdata.Add(paihang);
            }
            foreach (var item in paihangdata)
            {
                PaiHangList.Items.Add(item);
            }
            PaiHangList.SelectionMode = ListViewSelectionMode.Single;
            PaiHangList.SelectionChanged += PaiHangList_SelectionChanged;
            PaiHangLoadProgress.IsActive = false;
        }

        private void PaiHangList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if(list.SelectedIndex!=-1)
            {
                var data = list.SelectedItem as ViewMode.PaiHang;
                Frame.Navigate(typeof(page.YueKu.SongBanner), Class.data.ToJsonData(data));
                list.SelectedIndex = -1;
            }
        }

        public async void LoadGeDan(string categoryid = "")
        {
            GeDanLoadProgress.IsActive = true;
            var httpclient = new Noear.UWP.Http.AsyncHttpClient();
            httpclient.Url("http://mobilecdn.kugou.com/api/v3/category/special?pagesize=20&withsong=0&plat=0&categoryid="+ categoryid + "&sort="+gd_sort+"&page="+gd_page);
            var json = (await httpclient.Get()).GetString();
            json = json.Replace("{size}", "150");
            var obj = Windows.Data.Json.JsonObject.Parse(json);
            GeDanListData = Class.data.DataContractJsonDeSerialize<ObservableCollection<GeDanData>>(obj.GetNamedObject("data").GetNamedArray("info").ToString());
            foreach (var item in GeDanListData)
            {
                if(double.Parse(item.playcount)>10000)
                {
                    item.playcount = Math.Floor(double.Parse(item.playcount) / 10000).ToString() + "万";
                }
            }
            GeDanListView.ItemsSource = GeDanListData;
            GeDanLoadProgress.IsActive = false;
        }

        private void TopicList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            var data = list.SelectedItem as TuiJianData.TuiJian_topic;
            Frame.Navigate(typeof(page.WebPage), data.url);
        }

        private void Back_Btn_Click(object sender, RoutedEventArgs e)
        {
            var asd = new Windows.UI.Xaml.Media.Animation.NavigationThemeTransition();
            Frame.GoBack(asd.DefaultNavigationTransitionInfo);
        }

        private void SearchBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var asd = new Windows.UI.Xaml.Media.Animation.NavigationThemeTransition();
            Frame.Navigate(typeof(page.Search),null,asd.DefaultNavigationTransitionInfo);
        }

        private void TopBtnClicked(object sender, RoutedEventArgs e)
        {
            var btn=sender as Button;
            YueKuView.SelectedIndex = btn.TabIndex;
        }

        private void YueKuView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var flip = sender as FlipView;
                switch (flip.SelectedIndex)
                {
                    case 0:
                        TuiJian_Btn.BorderBrush = new SolidColorBrush(Colors.White);
                        PaiHang_Btn.BorderBrush = new SolidColorBrush();
                        GeShou_Btn.BorderBrush = new SolidColorBrush();
                        GeDan_Btn.BorderBrush = new SolidColorBrush();
                        if (alldata == null)
                        {
                            LoadTuiJianData();
                        }
                        break;
                    case 1:
                        TuiJian_Btn.BorderBrush = new SolidColorBrush();
                        PaiHang_Btn.BorderBrush = new SolidColorBrush(Colors.White);
                        GeShou_Btn.BorderBrush = new SolidColorBrush();
                        GeDan_Btn.BorderBrush = new SolidColorBrush();
                        if (paihangdata == null)
                        {
                            LoadBaiHangData();
                        }
                        break;
                    case 2:
                        TuiJian_Btn.BorderBrush = new SolidColorBrush();
                        PaiHang_Btn.BorderBrush = new SolidColorBrush();
                        GeShou_Btn.BorderBrush = new SolidColorBrush(Colors.White);
                        GeDan_Btn.BorderBrush = new SolidColorBrush();
                        break;
                    case 3:
                        TuiJian_Btn.BorderBrush = new SolidColorBrush();
                        PaiHang_Btn.BorderBrush = new SolidColorBrush();
                        GeShou_Btn.BorderBrush = new SolidColorBrush();
                        GeDan_Btn.BorderBrush = new SolidColorBrush(Colors.White);
                        if (GeDanListData == null)
                        {
                            LoadGeDan();
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                
            }
        }

        public class ViewMode
        {
            public class PaiHang
            {
                public string id { get; set; }
                public string rankid { get; set; }
                public string rankname { get; set; }
                public string imgurl { get; set; }
                public string song1 { get; set; }
                public string song2 { get; set; }
                public string song3 { get; set; }
            }
        }

        public class BannerData
        {
            public string imgurl { get; set; }
            public int type { get; set; }
            public ExtraData extra { get; set; }
            public class ExtraData
            {
                public string albumid { get; set; }
                public string innerurl { get; set; }
            }
        }

        public class TuiJianData
        {
            public List<TuiJian_song> song { get; set; }  //新歌首发
            public List<TuiJian_album> album { get; set; }  //新碟上架
            public List<TuiJian_recommend> recommend { get; set; }  //热门歌单
            public List<TuiJian_custom_special> custom_special { get; set; }  //主题歌单
            public List<TuiJian_vlist> vlist { get; set; }  //MV歌单
            public List<TuiJian_topic> topic { get; set; }  //精选主题

            public class TuiJian_song
            {
                public string filename { get; set; }
                public string singerimgurl { get; set; }
            }
            public class TuiJian_album
            {
                public string albumname { get; set; }
                public string singername { get; set; }
                public string albumid { get; set; }
                public string imgurl { get; set; }
            }
            public class TuiJian_recommend
            {
                public extradata extra { get; set; }
                public class extradata
                {
                    public string specialname { get; set; }
                    public string specialid { get; set; }
                    public string play_count { get; set; }
                    public string imgurl { get; set; }
                }
            }
            public class TuiJian_custom_special
            {
                public string title { get; set; }
                public string id { get; set; }
                public List<specialData> special { get; set; }
                public class specialData
                {
                    public string play_count { get; set; }
                    public string specialname { get; set; }
                    public string specialid { get; set; }
                    public string imgurl { get; set; }
                }
            }
            public class TuiJian_vlist
            {
                public string vid { get; set; }
                public string title { get; set; }
                public string public_time { get; set; }
                public string mobile_banner { get; set; }
            }
            public class TuiJian_topic
            {
                public string title { get; set; }
                public string publishtime { get; set; }
                public string url { get; set; }
                public string picurl { get; set; }
            }
        }

        public class PaiHangData
        {
            public int id { get; set; }
            public int rankid { get; set; }
            public string rankname { get; set; }
            public string imgurl { get; set; }
            public List<songs> songinfo { get; set; }
            public class songs
            {
                public string songname { get; set; }
            }
        }

        private void vlist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var data = grid.DataContext as TuiJianData.TuiJian_vlist;
            Frame.Navigate(typeof(page.MVListPage), data.vid);
        }

        public class GeDanData
        {
            public string specialid { get; set; }
            public string specialname { get; set; }
            public string playcount { get; set; }
            public string imgurl { get; set; }
        }
    }
}
