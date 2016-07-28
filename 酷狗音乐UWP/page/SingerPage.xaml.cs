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
using Windows.UI.Popups;
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
    public sealed partial class SingerPage : Page
    {
        private SongListData songlistdata;
        private MVListData mvlistdata;
        private AlbumListData albumlistdata;

        public SingerPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadProcess.IsActive = true;
            var singerid = e.Parameter.ToString();

            songlistdata = new SongListData(singerid);
            SongList.ItemsSource = songlistdata.List;
            SongList.SelectionMode = ListViewSelectionMode.Single;
            SongList.SelectionChanged += SongList_SelectionChanged;
            await songlistdata.LoadData();

            mvlistdata = new MVListData(singerid);
            MVList.ItemsSource = mvlistdata.List;
            MVList.SelectionMode = ListViewSelectionMode.Single;
            MVList.SelectionChanged += MVList_SelectionChanged;

            albumlistdata = new AlbumListData(singerid);
            AlbumList.ItemsSource = albumlistdata.List;
            AlbumList.SelectionMode = ListViewSelectionMode.Single;
            AlbumList.SelectionChanged += AlbumList_SelectionChanged;

            flipview.SelectionChanged += flipview_SelectionChanged;
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var applicationView = ApplicationView.GetForCurrentView();
                applicationView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = Colors.White;
                statusBar.BackgroundColor = Color.FromArgb(1, 68, 190, 239);
                statusBar.BackgroundOpacity = 0;
            }
            singerinfo_Grid.DataContext = await GetSingerInfo(singerid);
            LoadProcess.IsActive = false;
        }

        private void MVList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if(list.SelectedItem!=null)
            {
                var data = list.SelectedItem as MVListData.MVData;
                Frame.Navigate(typeof(page.MVPlayer), data.hash);
                list.SelectedIndex = -1;
            }
        }

        private void AlbumList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as GridView;
            if (list.SelectedItem != null)
            {
                var data = list.SelectedItem as AlbumListData.AlbumData;
                Frame.Navigate(typeof(page.AlbumPage), data.albumid);
                list.SelectedIndex = -1;
            }
        }

        private async void SongList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectionMode!= ListViewSelectionMode.Multiple && list.SelectedItem != null)
            {
                var data = list.SelectedItem as SongListData.SongData;
                LoadProcess.IsActive = true;
                await data.AddToPlayList(true);
                LoadProcess.IsActive = false;
                list.SelectedIndex = -1;
            }
        }

        private async Task<SingerInfo> GetSingerInfo(string singerid)
        {
            var httpclient = new Noear.UWP.Http.AsyncHttpClient();
            httpclient.Url("http://mobilecdn.kugou.com/api/v3/singer/info?singerid=" + singerid);
            var json = (await httpclient.Get()).GetString();
            json = json.Replace("{size}", "400");
            var obj = Windows.Data.Json.JsonObject.Parse(json);
            var data = Class.data.DataContractJsonDeSerialize<SingerInfo>(obj.GetNamedObject("data").ToString());
            return data;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var Theme = (Application.Current.Resources.ThemeDictionaries.ToList())[0].Value as ResourceDictionary;
                var applicationView = ApplicationView.GetForCurrentView();
                applicationView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = Colors.White;
                statusBar.BackgroundColor = ((SolidColorBrush)Theme["KuGou-BackgroundColor"]).Color;
                statusBar.BackgroundOpacity = 100;
            }
        }

        public class SingerInfo
        {
            public string singerid { get; set; }
            public string singername { get; set; }
            public string intro { get; set; }
            public string songcount { get; set; }
            public string albumcount { get; set; }
            public string mvcount { get; set; }
            public string imgurl { get; set; }
        }

        public class SongListData
        {
            public ObservableCollection<SongData> List { get; set; }
            public int page = 0;
            private string singerid { get; set; }
            public SortType sort = SortType.最热;
            public enum SortType
            {
                最新=1,最热=2
            }
            public class SongData:Class.Model.ISong
            {
                public string filename { get; set; }
                public string hash { get; set; }
                public string mvhash { get; set; }
                public string sqhash { get; set; }
                public string hash320 { get; set; }
                public string album_id { get; set; }
                public string mvview
                {
                    get
                    {
                        if(mvhash!=null&&mvhash!="")
                        {
                            return "Visible";
                        }
                        else
                        {
                            return "Collapsed";
                        }
                    }
                }
                public string songname
                {
                    get
                    {
                        if(filename.Contains("-"))
                        {
                            return filename.Split('-')[1].Replace(" ", "");
                        }
                        else
                        {
                            return filename;
                        }
                    }
                }
                public string singername
                {
                    get
                    {
                        if (filename.Contains("-"))
                        {
                            return filename.Split('-')[0].Replace(" ", "");
                        }
                        else
                        {
                            return "";
                        }
                    }
                }
                public async Task<string> GetUrl()
                {
                    if (hash != "")
                    {
                        switch (Class.Setting.Qu.GetType())
                        {
                            case Class.Setting.Qu.Type.low:
                                return await Class.kugou.get_musicurl_by_hash(hash);
                            case Class.Setting.Qu.Type.mid:
                                if (hash320 != "")
                                {
                                    hash = hash320;
                                    return await Class.kugou.get_musicurl_by_hash(hash320);
                                }
                                else
                                {
                                    return await Class.kugou.get_musicurl_by_hash(hash);
                                }
                            case Class.Setting.Qu.Type.high:
                                if (sqhash != null)
                                {
                                    hash = sqhash;
                                    return await Class.kugou.get_musicurl_by_hash(sqhash);
                                }
                                else
                                {
                                    if (hash320 != "")
                                    {
                                        hash = hash320;
                                        return await Class.kugou.get_musicurl_by_hash(hash320);
                                    }
                                    else
                                    {
                                        return await Class.kugou.get_musicurl_by_hash(hash);
                                    }
                                }
                            default:
                                return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                public async Task<Model.Player.NowPlay> GetNowPlay()
                {
                    var music = new Class.Model.Player.NowPlay();
                    music.title = filename;
                    music.url = await GetUrl();
                    music.hash = hash;
                    if (album_id != null && album_id != "" && album_id != "0")
                    {
                        music.albumid = album_id;
                    }
                    else
                    {
                        music.albumid = "";
                    }
                    if (singername.Length > 0)
                    {
                        music.singername = singername;
                        var singer = await Class.Model.SearchResultModel.GetSingerResult(singername);
                        if (singer == null)
                        {
                            music.imgurl = "ms-appx:///Assets/image/songimg.png";
                        }
                        else
                        {
                            music.imgurl = singer.imgurl;
                        }
                    }
                    else
                    {
                        music.singername = "未知歌手";
                        music.imgurl = "ms-appx:///Assets/image/songimg.png";
                    }
                    return music;
                }
                public async Task AddToPlayList(bool isplay)
                {
                    var nowplay = await this.GetNowPlay();
                    if (nowplay.url == null || nowplay.url == "")
                    {
                        await new MessageDialog("该音乐暂时无法播放！").ShowAsync();
                    }
                    else
                    {
                        await Class.Model.PlayList.Add(nowplay, isplay);
                    }
                }
                public async Task AddToDownloadList()
                {
                    var url = await GetDownUrl();
                    if (url != null && url != "")
                    {
                        await KG_ClassLibrary.BackgroundDownload.Start(filename, url, KG_ClassLibrary.BackgroundDownload.DownloadType.song);
                    }
                }
                public async Task<string> GetDownUrl()
                {
                    if (hash != "")
                    {
                        switch (Class.Setting.DownQu.GetType())
                        {
                            case Class.Setting.DownQu.Type.low:
                                return await Class.kugou.get_musicurl_by_hash(hash);
                            case Class.Setting.DownQu.Type.mid:
                                if (hash320 != "")
                                {
                                    return await Class.kugou.get_musicurl_by_hash(hash320);
                                }
                                else
                                {
                                    return await Class.kugou.get_musicurl_by_hash(hash);
                                }
                            case Class.Setting.DownQu.Type.high:
                                if (sqhash != null)
                                {
                                    return await Class.kugou.get_musicurl_by_hash(sqhash);
                                }
                                else
                                {
                                    if (hash320 != "")
                                    {
                                        return await Class.kugou.get_musicurl_by_hash(hash320);
                                    }
                                    else
                                    {
                                        return await Class.kugou.get_musicurl_by_hash(hash);
                                    }
                                }
                            default:
                                return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            public SongListData(string id)
            {
                singerid = id;
                List = new ObservableCollection<SongData>();
            }
            public async Task LoadData()
            {
                page = page + 1;
                var httpclient = new Noear.UWP.Http.AsyncHttpClient();
                httpclient.Url("http://mobilecdn.kugou.com/api/v3/singer/song?pagesize=20&singerid="+singerid+"&plat=0&page="+page.ToString()+"&sorttype="+((int)sort).ToString() +"&version=8150");
                var json = (await httpclient.Get()).GetString();
                json = json.Replace("320hash", "hash320");
                var obj = Windows.Data.Json.JsonObject.Parse(json);
                var data = Class.data.DataContractJsonDeSerialize<ObservableCollection<SongData>>(obj.GetNamedObject("data").GetNamedArray("info").ToString());
                foreach (var item in data)
                {
                    List.Add(item);
                }
            }
        }

        public class MVListData
        {
            public ObservableCollection<MVData> List { get; set; }
            public int page = 0;
            private string singerid { get; set; }
            public class MVData
            {
                public string filename { get; set; }
                public string singername { get; set; }
                public string hash { get; set; }
                public string imgurl { get; set; }
                public string intro { get; set; }
            }
            public MVListData(string id)
            {
                singerid = id;
                List = new ObservableCollection<MVData>();
            }
            public async Task LoadData()
            {
                page = page + 1;
                var httpclient = new Noear.UWP.Http.AsyncHttpClient();
                httpclient.Url("http://mobilecdn.kugou.com/api/v3/singer/mv?pagesize=20&singerid="+singerid+"&plat=0&page="+page.ToString()+"&version=8150");
                var json = (await httpclient.Get()).GetString();
                var obj = Windows.Data.Json.JsonObject.Parse(json);
                var data = Class.data.DataContractJsonDeSerialize<ObservableCollection<MVData>>(obj.GetNamedObject("data").GetNamedArray("info").ToString());
                foreach (var item in data)
                {
                    List.Add(item);
                }
            }
        }

        public class AlbumListData
        {
            public ObservableCollection<AlbumData> List { get; set; }
            public int page = 0;
            private string singerid { get; set; }
            public class AlbumData
            {
                public string albumname { get; set; }
                public string albumid { get; set; }
                public string singername { get; set; }
                public string publishtime { get; set; }
                public string imgurl { get; set; }
            }
            public AlbumListData(string id)
            {
                singerid = id;
                List = new ObservableCollection<AlbumData>();
            }
            public async Task LoadData()
            {
                page = page + 1;
                var httpclient = new Noear.UWP.Http.AsyncHttpClient();
                httpclient.Url("http://mobilecdn.kugou.com/api/v3/singer/album?pagesize=20&singerid=" + singerid + "&plat=0&page=" + page.ToString() + "&version=8150");
                var json = (await httpclient.Get()).GetString();
                json = json.Replace("{size}", "150");
                json = json.Replace(" 00:00:00", "");
                var obj = Windows.Data.Json.JsonObject.Parse(json);
                var data = Class.data.DataContractJsonDeSerialize<ObservableCollection<AlbumData>>(obj.GetNamedObject("data").GetNamedArray("info").ToString());
                foreach (var item in data)
                {
                    List.Add(item);
                }
            }
        }

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void flipview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var view = sender as FlipView;
            var Theme = (Application.Current.Resources.ThemeDictionaries.ToList())[0].Value as ResourceDictionary;
            switch (view.SelectedIndex)
            {
                case 0:
                    if (songlistdata.page == 0)
                    {
                        LoadProcess.IsActive = true;
                        await songlistdata.LoadData();
                        LoadProcess.IsActive = false;
                    }
                    TopSongBtn.BorderBrush= (SolidColorBrush)Theme["KuGou-Foreground"];
                    TopAlbumBtn.BorderBrush = null;
                    TopMVBtn.BorderBrush = null;
                    TopMoreBtn.BorderBrush = null;
                    break;
                case 1:
                    if (albumlistdata.page == 0)
                    {
                        LoadProcess.IsActive = true;
                        await albumlistdata.LoadData();
                        LoadProcess.IsActive = false;
                    }
                    TopSongBtn.BorderBrush = null;
                    TopAlbumBtn.BorderBrush = (SolidColorBrush)Theme["KuGou-Foreground"];
                    TopMVBtn.BorderBrush = null;
                    TopMoreBtn.BorderBrush = null;
                    break;
                case 2:
                    if(mvlistdata.page==0)
                    {
                        LoadProcess.IsActive = true;
                        await mvlistdata.LoadData();
                        LoadProcess.IsActive = false;
                    }
                    TopSongBtn.BorderBrush = null;
                    TopAlbumBtn.BorderBrush = null;
                    TopMVBtn.BorderBrush = (SolidColorBrush)Theme["KuGou-Foreground"];
                    TopMoreBtn.BorderBrush = null;
                    break;
                case 3:
                    TopSongBtn.BorderBrush = null;
                    TopAlbumBtn.BorderBrush = null;
                    TopMVBtn.BorderBrush = null;
                    TopMoreBtn.BorderBrush = (SolidColorBrush)Theme["KuGou-Foreground"];
                    break;
                default:
                    break;
            }
        }

        private async void NextPage(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv_SP = sender as ScrollViewer;
            if (sv_SP.VerticalOffset == sv_SP.ScrollableHeight)
            {
                switch (sv_SP.TabIndex)
                {
                    case 0:
                        if (!LoadProcess.IsActive)
                        {
                            LoadProcess.IsActive = true;
                            await songlistdata.LoadData();
                            LoadProcess.IsActive = false;
                        }
                        break;
                    case 1:
                        if (!LoadProcess.IsActive)
                        {
                            LoadProcess.IsActive = true;
                            await albumlistdata.LoadData();
                            LoadProcess.IsActive = false;
                        }
                        break;
                    case 2:
                        if (!LoadProcess.IsActive)
                        {
                            LoadProcess.IsActive = true;
                            await mvlistdata.LoadData();
                            LoadProcess.IsActive = false;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void TopBtnCLicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            flipview.SelectedIndex = btn.TabIndex;
        }

        private async void SongSortChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ComboBox;
            if (songlistdata != null)
            {
                songlistdata.sort = (SongListData.SortType)list.SelectedIndex;
                LoadProcess.IsActive = true;
                songlistdata.List.Clear();
                songlistdata.page = 0;
                await songlistdata.LoadData();
                LoadProcess.IsActive = false;
            }
        }

        private void MoreSongBtn_Click(object sender, RoutedEventArgs e)
        {
            if (songlistdata != null && songlistdata.List.Count > 0)
            {
                if(SongList.SelectionMode==ListViewSelectionMode.Single)
                {
                    SongList.SelectionMode = ListViewSelectionMode.Multiple;
                    MoreSongBox.Show();
                    MoreSongBox.BtnClickedEvent += MoreSongBox_BtnClickedEvent;
                }
                else
                {
                    SongList.SelectionMode = ListViewSelectionMode.Single;
                    MoreSongBox.Hidden();
                }
            }
        }

        private async void MoreSongBox_BtnClickedEvent(UserControlClass.SongMultipleBox.BtnType type)
        {
            LoadProcess.IsActive = true;
            if (SongList.SelectedItems != null && SongList.SelectedItems.Count > 0)
            {
                switch (type)
                {
                    case UserControlClass.SongMultipleBox.BtnType.NextPlay:
                        foreach (var item in SongList.SelectedItems)
                        {
                            var song = item as SongListData.SongData;
                            await song.AddToPlayList(false);
                        }
                        break;
                    case UserControlClass.SongMultipleBox.BtnType.Download:
                        foreach (var item in SongList.SelectedItems)
                        {
                            var song = item as SongListData.SongData;
                            await song.AddToDownloadList();
                        }
                        break;
                    case UserControlClass.SongMultipleBox.BtnType.AddToList:
                        break;
                    default:
                        break;
                }
            }
            MoreSongBox.Hidden();
            SongList.SelectionMode = ListViewSelectionMode.Single;
            LoadProcess.IsActive = false;
        }
    }
}
