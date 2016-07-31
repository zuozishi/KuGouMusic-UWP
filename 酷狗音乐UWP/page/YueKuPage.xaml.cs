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
        private List<BannerData> bannerdata;
        private GeDanList gedandata;

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
            LoadGeDanType();
            gedandata = new GeDanList();
            await gedandata.LoadData();
            GeDanSortBox.SelectionChanged += GeDanSortBox_SelectionChanged;
            GeDanListView.ItemsSource = gedandata.List;
            GeDanLoadProgress.IsActive = false;
        }

        private async void LoadGeDanType()
        {
            var firstmenu = new MenuFlyoutItem() { Text = "全部分类", Tag = "" };
            firstmenu.Click += GeDanTypeClicked;
            GeDanTypeMenu.Items.Add(firstmenu);
            var httpclient = new Noear.UWP.Http.AsyncHttpClient();
            httpclient.Url("http://mobilecdn.kugou.com/api/v3/tag/list?pid=0&plat=0&apiver=2");
            var json = (await httpclient.Get()).GetString();
            var obj = Windows.Data.Json.JsonObject.Parse(json);
            var data = Class.data.DataContractJsonDeSerialize<List<GeDanTypeData>>(obj.GetNamedObject("data").GetNamedArray("info").ToString());
            data.RemoveAt(0);
            foreach (var group in data)
            {
                var list = new MenuFlyoutSubItem();
                list.Text = group.name;
                foreach (var item in group.children)
                {
                    var menu = new MenuFlyoutItem();
                    menu.Text = item.name;
                    menu.Tag = item.special_tag_id;
                    menu.Click += GeDanTypeClicked;
                    list.Items.Add(menu);
                }
                GeDanTypeMenu.Items.Add(list);
            }
        }

        private async void GeDanTypeClicked(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuFlyoutItem;
            if (gedandata != null)
            {
                GeDanLoadProgress.IsActive = true;
                gedandata.page = 0;
                gedandata.categoryid = menu.Tag.ToString();
                gedandata.List.Clear();
                GeDanTypeBtn.Content = menu.Text;
                await gedandata.LoadData();
                GeDanLoadProgress.IsActive = false;
            }
        }

        public class GeDanTypeData
        {
            public string name { get; set; }
            public List<Data> children { get; set; }
            public class Data
            {
                public string name { get; set; }
                public string special_tag_id { get; set; }
            }
        }

        private void TopicList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedItem != null)
            {
                var data = list.SelectedItem as TuiJianData.TuiJian_topic;
                Frame.Navigate(typeof(page.WebPage), data.url);
                list.SelectedIndex = -1;
            }
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
                var Theme = (Application.Current.Resources.ThemeDictionaries.ToList())[0].Value as ResourceDictionary;
                var flip = sender as FlipView;
                switch (flip.SelectedIndex)
                {
                    case 0:
                        TuiJian_Btn.BorderBrush = (SolidColorBrush)Theme["KuGou-Foreground"];
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
                        PaiHang_Btn.BorderBrush = (SolidColorBrush)Theme["KuGou-Foreground"];
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
                        GeShou_Btn.BorderBrush = (SolidColorBrush)Theme["KuGou-Foreground"];
                        GeDan_Btn.BorderBrush = new SolidColorBrush();
                        break;
                    case 3:
                        TuiJian_Btn.BorderBrush = new SolidColorBrush();
                        PaiHang_Btn.BorderBrush = new SolidColorBrush();
                        GeShou_Btn.BorderBrush = new SolidColorBrush();
                        GeDan_Btn.BorderBrush = (SolidColorBrush)Theme["KuGou-Foreground"];
                        if (gedandata == null)
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

        private void SingerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            switch (list.SelectedIndex)
            {
                case 1:
                    Frame.Navigate(typeof(page.YueKu.SingerListPage), new string[] { "1", "1","华语男歌手" });
                    break;
                case 2:
                    Frame.Navigate(typeof(page.YueKu.SingerListPage), new string[] { "1", "2", "华语女歌手" });
                    break;
                case 3:
                    Frame.Navigate(typeof(page.YueKu.SingerListPage), new string[] { "1", "3", "华语组合" });
                    break;
                case 4:
                    Frame.Navigate(typeof(page.YueKu.SingerListPage), new string[] { "6", "1", "韩国男歌手" });
                    break;
                case 5:
                    Frame.Navigate(typeof(page.YueKu.SingerListPage), new string[] { "6", "2", "韩国女歌手" });
                    break;
                case 6:
                    Frame.Navigate(typeof(page.YueKu.SingerListPage), new string[] { "6", "3", "韩国组合" });
                    break;
                case 7:
                    Frame.Navigate(typeof(page.YueKu.SingerListPage), new string[] { "5", "1", "日本男歌手" });
                    break;
                case 8:
                    Frame.Navigate(typeof(page.YueKu.SingerListPage), new string[] { "5", "2", "日本女歌手" });
                    break;
                case 9:
                    Frame.Navigate(typeof(page.YueKu.SingerListPage), new string[] { "5", "3", "日本组合" });
                    break;
                case 10:
                    Frame.Navigate(typeof(page.YueKu.SingerListPage), new string[] { "2", "1", "欧美男歌手" });
                    break;
                case 11:
                    Frame.Navigate(typeof(page.YueKu.SingerListPage), new string[] { "2", "2", "欧美女歌手" });
                    break;
                case 12:
                    Frame.Navigate(typeof(page.YueKu.SingerListPage), new string[] { "2", "3", "欧美组合" });
                    break;
                case 13:
                    Frame.Navigate(typeof(page.YueKu.SingerListPage), new string[] { "4", "0", "其他歌手" });
                    break;
                case 14:
                    Frame.Navigate(typeof(page.YueKu.SingerListPage), new string[] { "0", "0", "酷狗音乐人" });
                    break;
                default:
                    break;
            }
            list.SelectedIndex = -1;
        }

        public class GeDanData
        {
            public string specialid { get; set; }
            public string specialname { get; set; }
            public string playcount { get; set; }
            public string imgurl { get; set; }
        }

        public class GeDanList
        {
            public ObservableCollection<GeDanData> List=new ObservableCollection<GeDanData>();
            public int page = 0;
            public string categoryid = "";
            public Type sort = Type.最热;
            public enum Type
            {
                最新=2,最热=3
            }
            public async Task LoadData()
            {
                page = page + 1;
                var httpclient = new Noear.UWP.Http.AsyncHttpClient();
                httpclient.Url("http://mobilecdn.kugou.com/api/v3/category/special?pagesize=20&withsong=0&plat=0&categoryid=" + categoryid + "&sort=" + ((int)sort).ToString() + "&page=" + page.ToString());
                var json = (await httpclient.Get()).GetString();
                json = json.Replace("{size}", "150");
                var obj = Windows.Data.Json.JsonObject.Parse(json);
                var data = Class.data.DataContractJsonDeSerialize<ObservableCollection<GeDanData>>(obj.GetNamedObject("data").GetNamedArray("info").ToString());
                foreach (var item in data)
                {
                    if (double.Parse(item.playcount) > 10000)
                    {
                        item.playcount = Math.Floor(double.Parse(item.playcount) / 10000).ToString() + "万";
                    }
                    List.Add(item);
                }
            }
        }

        private async void GeDanSortBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var box = sender as ComboBox;
            GeDanLoadProgress.IsActive = true;
            switch (box.SelectedIndex)
            {
                case 0:
                    gedandata.page = 0;
                    gedandata.List.Clear();
                    gedandata.sort = GeDanList.Type.最新;
                    await gedandata.LoadData();
                    break;
                case 1:
                    gedandata.page = 0;
                    gedandata.List.Clear();
                    gedandata.sort = GeDanList.Type.最热;
                    await gedandata.LoadData();
                    break;
                default:break;
            }
            GeDanLoadProgress.IsActive = false;
        }

        private async void GeDanLoadMore(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var view = sender as ScrollViewer;
            if (view.VerticalOffset == view.ScrollableHeight)
            {
                if (!GeDanLoadProgress.IsActive)
                {
                    GeDanLoadProgress.IsActive = true;
                    await gedandata.LoadData();
                    GeDanLoadProgress.IsActive = false;
                }
            }
        }

        private void GeDanListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedItem != null)
            {
                var data = list.SelectedItem as GeDanData;
                Frame.Navigate(typeof(page.SongListPage),new string[] { data.specialid,data.specialname});
            }
        }

        private void MVBtn_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(page.YueKu.MVPage));
        }

        private void HotGeDan_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var grid = sender as Grid;
            if (grid.DataContext != null)
            {
                var data = grid.DataContext as TuiJianData.TuiJian_recommend.extradata;
                Frame.Navigate(typeof(page.SongListPage), new string[] { data.specialid, data.specialname });
            }
        }

        private void Special_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var grid = sender as Grid;
            if (grid.DataContext != null)
            {
                var data = grid.DataContext as TuiJianData.TuiJian_custom_special.specialData;
                Frame.Navigate(typeof(page.SongListPage), new string[] { data.specialid,data.specialname});
            }
        }
    }
}
