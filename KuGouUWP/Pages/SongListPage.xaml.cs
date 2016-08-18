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

namespace KuGouUWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SongListPage : Page
    {
        public SongListPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadProcess.IsActive = true;
            var data = e.Parameter as string[];
            var id = data[0];
            title.Text = data[1];
            var songlistdata = await SongLsitData.GetData(id);
            if (songlistdata != null)
            {
                SongListView.ItemsSource = songlistdata.List;
                SongListView.SelectionMode = ListViewSelectionMode.Single;
                SongListView.SelectionChanged += SongListView_SelectionChanged;
            }
            LoadProcess.IsActive = false;
        }

        private async void SongListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProcess.IsActive = true;
            var list = sender as ListView;
            if (list.SelectedItem != null&&list.SelectionMode==ListViewSelectionMode.Single)
            {
                if (list.SelectionMode == ListViewSelectionMode.Single)
                {
                    var data = list.SelectedItem as SongLsitData.SongData;
                    await data.AddToPlayList(true);
                }
            }
            LoadProcess.IsActive = false;
        }

        public class SongLsitData
        {
            public ObservableCollection<SongData> List { get; set; }
            public class SongData:Class.Model.ISong
            {
                public string filename { get; set; }
                public string hash { get; set; }
                public string mvhash { get; set; }
                public string hash320 { get; set; }
                public string sqhash { get; set; }
                public string album_id { get; set; }
                public string hashmv
                {
                    get
                    {
                        if (mvhash != "")
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
                        if (filename.Contains("-"))
                        {
                            return filename.Split('-')[1].Replace(" ","");
                        }else
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
                        var type = Class.Setting.Qu.GetType();
                        type = Class.Setting.Qu.Type.low;
                        switch (type)
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
                public async Task<Class.Model.Player.NowPlay> GetNowPlay()
                {
                    var music = new Class.Model.Player.NowPlay();
                    music.title = filename;
                    music.url = await GetUrl();
                    if (album_id != "0"&& album_id != "")
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
                        
                        switch (Class.Setting.DownQu.Type.low)
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
            public static async Task<SongLsitData> GetData(string id)
            {
                try
                {
                    var httpclient = new System.Net.Http.HttpClient();
                    var json = await httpclient.GetStringAsync("http://mobilecdn.kugou.com/api/v3/special/song?pagesize=-1&plat=0&page=1&version=8150&specialid=" + id);
                    json = json.Replace("320hash", "hash320");
                    var obj = Windows.Data.Json.JsonObject.Parse(json);
                    var data = new SongLsitData();
                    data.List = Class.data.DataContractJsonDeSerialize<ObservableCollection<SongData>>(obj.GetNamedObject("data").GetNamedArray("info").ToString());
                    return data;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void MoreBtn_Clicked(object sender, RoutedEventArgs e)
        {
            if (SongListView.ItemsSource != null)
            {
                if (SongListView.SelectionMode == ListViewSelectionMode.Single)
                {
                    SongListView.SelectionMode = ListViewSelectionMode.Multiple;
                    MoreSongBox.Show();
                    MoreSongBox.BtnClickedEvent += MoreSongBox_BtnClickedEvent;
                }
                else
                {
                    SongListView.SelectionMode = ListViewSelectionMode.Single;
                    MoreSongBox.Hidden();
                    MoreSongBox.BtnClickedEvent -= MoreSongBox_BtnClickedEvent;
                }
            }
        }

        private async void MoreSongBox_BtnClickedEvent(UserControlClass.SongMultipleBox.BtnType type)
        {
            LoadProcess.IsActive = true;
            switch (type)
            {
                case UserControlClass.SongMultipleBox.BtnType.NextPlay:
                    foreach (var item in SongListView.SelectedItems)
                    {
                        var music = item as SongLsitData.SongData;
                        await music.AddToPlayList(false);
                    }
                    await new MessageDialog("已添加到播放列表").ShowAsync();
                    SongListView.SelectionMode = ListViewSelectionMode.Single;
                    MoreSongBox.Hidden();
                    MoreSongBox.BtnClickedEvent -= MoreSongBox_BtnClickedEvent;
                    break;
                case UserControlClass.SongMultipleBox.BtnType.Download:
                    foreach (var item in SongListView.SelectedItems)
                    {
                        var music = item as SongLsitData.SongData;
                        await music.AddToDownloadList();
                    }
                    await new MessageDialog("已添加到下载列表").ShowAsync();
                    SongListView.SelectionMode = ListViewSelectionMode.Single;
                    MoreSongBox.Hidden();
                    MoreSongBox.BtnClickedEvent -= MoreSongBox_BtnClickedEvent;
                    break;
                case UserControlClass.SongMultipleBox.BtnType.AddToList:
                    SongListView.SelectionMode = ListViewSelectionMode.Single;
                    MoreSongBox.Hidden();
                    MoreSongBox.BtnClickedEvent -= MoreSongBox_BtnClickedEvent;
                    break;
                case UserControlClass.SongMultipleBox.BtnType.SelectAll:
                    if (SongListView.SelectedItems.Count == 0)
                    {
                        SongListView.SelectAll();
                    }else
                    {
                        SongListView.SelectedItems.Clear();
                    }
                    break;
                default:
                    break;
            }
            LoadProcess.IsActive = false;
        }
    }
}
