using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using KuGouUWP.Class;
using Windows.ApplicationModel.Resources;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace KuGouUWP.Pages.YueKu
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SongBanner : Page
    {
        private SongDataList listmanager;
        private object o = new object();

        public SongBanner()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            var json = e.Parameter as string;
            var paihangdata = Class.data.DataContractJsonDeSerialize<YueKuPage.ViewMode.PaiHang>(json);
            title.Text = paihangdata.rankname;
            listmanager = new SongDataList(paihangdata.rankid);
            SongLoadProgress.IsActive = true;
            await listmanager.LoadPage();
            SongListView.ItemsSource = listmanager.list;
            SongListView.SelectionMode = ListViewSelectionMode.Single;
            SongListView.SelectionChanged += SongListView_SelectionChanged;
            SongLoadProgress.IsActive = false;
        }

        private async void SongListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SongLoadProgress.IsActive = true;
            var list = sender as ListView;
            if(list.SelectedIndex!=-1)
            {
                if (list.SelectionMode != ListViewSelectionMode.Multiple && list.SelectedItem != null)
                {
                    var data = list.SelectedItem as SongData;
                    await data.AddToPlayList(true);
                    SongListView.SelectedIndex = -1;
                }
            }
            SongLoadProgress.IsActive = false;
        }

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv_SP = sender as ScrollViewer;
            if (sv_SP.VerticalOffset == sv_SP.ScrollableHeight)
            {
                lock (o)
                {
                    if (!SongLoadProgress.IsActive)
                    {
                        SongLoadProgress.IsActive = true;
                        Task.Factory.StartNew(async () =>
                        {
                            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,async () =>
                            {
                                await listmanager.LoadPage();
                                SongLoadProgress.IsActive = false;
                            });
                        });
                    }
                }
            }
        }

        public class SongData:Class.Model.ISong
        {
            public string filename { get; set; }
            public string songname
            {
                get
                {
                    if (filename.Contains("-"))
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
            public string sqhash { get; set; }
            public string hash { get; set; }
            public string album_id { get; set; }
            public string mvhash { get; set; }
            public string hash320 { get; set; }
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
                if(album_id==null||album_id==""||album_id=="0")
                {
                    music.albumid = "";
                }
                else
                {
                    music.albumid = album_id;
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
                try
                {
                    var nowplay = await this.GetNowPlay();
                    if (nowplay.url == null || nowplay.url == "")
                    {
                        await new MessageDialog(ResourceLoader.GetForCurrentView().GetString("PlayFalied")).ShowAsync();
                    }
                    else
                    {
                        await Class.Model.PlayList.Add(nowplay, isplay);
                    }
                }
                catch (Exception)
                {
                    
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

        public class SongDataList
        {
            public ObservableCollection<SongData> list { get; set; }
            public int page = 0;
            private string id;
            public SongDataList(string rankid)
            {
                list = new ObservableCollection<SongData>();
                id = rankid;
            }
            public async Task LoadPage()
            {
                try
                {
                    page = page + 1;
                    var httpclient = new System.Net.Http.HttpClient();
                    var json = await httpclient.GetStringAsync("http://mobilecdn.kugou.com/api/v3/rank/song?rankid=" + id + "&pagesize=20&plat=0&ranktype=2&page=" + page.ToString() + "&version=8150");
                    json = json.Replace("320hash", "hash320");
                    var obj = Windows.Data.Json.JsonObject.Parse(json);
                    var data = Class.data.DataContractJsonDeSerialize<ObservableCollection<SongData>>(obj.GetNamedObject("data").GetNamedArray("info").ToString());
                    foreach (var item in data)
                    {
                        list.Add(item);
                    }
                }
                catch (Exception)
                {
                    page = page - 1;
                }
            }
        }

        private void TopMoreBtn_Click(object sender, RoutedEventArgs e)
        {
            if(SongListView.SelectionMode == ListViewSelectionMode.Single)
            {
                SongListView.SelectionMode = ListViewSelectionMode.Multiple;
                MoreSongBox.Show();
                MoreSongBox.BtnClickedEvent += MoreSongBox_BtnClickedEvent;
            }
            else
            {
                SongListView.SelectionMode = ListViewSelectionMode.Single;
                MoreSongBox.Hidden();
            }
        }

        private async void MoreSongBox_BtnClickedEvent(UserControlClass.SongMultipleBox.BtnType type)
        {
            if (SongListView.SelectedItems != null)
            {
                SongLoadProgress.IsActive = true;
                switch (type)
                {
                    case UserControlClass.SongMultipleBox.BtnType.NextPlay:
                        foreach (var item in SongListView.SelectedItems)
                        {
                            var song = item as SongData;
                            await song.AddToPlayList(false);
                        }
                        break;
                    case UserControlClass.SongMultipleBox.BtnType.Download:
                        foreach (var item in SongListView.SelectedItems)
                        {
                            var song = item as SongData;
                            await song.AddToDownloadList();
                        }
                        break;
                    case UserControlClass.SongMultipleBox.BtnType.AddToList:
                        break;
                    case UserControlClass.SongMultipleBox.BtnType.SelectAll:
                        if (SongListView.SelectedItems.Count == 0)
                        {
                            SongListView.SelectAll();
                        }
                        else
                        {
                            SongListView.SelectedItems.Clear();
                        }
                        break;
                    default:
                        break;
                }
                if (type != UserControlClass.SongMultipleBox.BtnType.SelectAll)
                {
                    MoreSongBox.Hidden();
                    SongListView.SelectionMode = ListViewSelectionMode.Single;
                }
                SongLoadProgress.IsActive = false;
            }
        }
    }
}
