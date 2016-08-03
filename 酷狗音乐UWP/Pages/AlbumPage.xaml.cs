using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace KuGouMusicUWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AlbumPage : Page
    {
        public AlbumPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadProcess.IsActive = true;
            var albumid = e.Parameter.ToString();
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var applicationView = ApplicationView.GetForCurrentView();
                applicationView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = Colors.White;
                statusBar.BackgroundColor = Color.FromArgb(1, 68, 190, 239);
                statusBar.BackgroundOpacity = 0;
            }
            albuminfo_Grid.DataContext = await GetAlbumInfo(albumid);
            SongList.ItemsSource = await GetSongList(albumid);
            SongList.SelectionMode = ListViewSelectionMode.Single;
            SongList.SelectionChanged += SongList_SelectionChanged;
            LoadProcess.IsActive = false;
        }

        private async void SongList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProcess.IsActive = true;
            var list = sender as ListView;
            if(list.SelectedItem!=null)
            {
                var song = list.SelectedItem as SongData;
                await song.AddToPlayList(true);
                list.SelectedIndex = -1;
            }
            LoadProcess.IsActive = false;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var applicationView = ApplicationView.GetForCurrentView();
                applicationView.SetDesiredBoundsMode(ApplicationViewBoundsMode.UseVisible);
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = Colors.White;
                statusBar.BackgroundColor = Color.FromArgb(1, 68, 190, 239);
                statusBar.BackgroundOpacity = 100;
            }
        }

        private async Task<Album_Info> GetAlbumInfo(string albumid)
        {
            var httpclient = new Noear.UWP.Http.AsyncHttpClient();
            httpclient.Url("http://mobilecdn.kugou.com/api/v3/album/info?albumid="+albumid);
            var json = (await httpclient.Get()).GetString();
            json = json.Replace("{size}", "400");
            var obj = Windows.Data.Json.JsonObject.Parse(json);
            var data = Class.data.DataContractJsonDeSerialize<Album_Info>(obj.GetNamedObject("data").ToString());
            data.publishtime = data.publishtime.Replace("00:00:00", "");
            data.publishtime = data.publishtime + "发行";
            GetSingerInfo(data.singerid);
            return data;
        }

        public async Task<List<SongData>> GetSongList(string albumid)
        {
            var httpclient = new Noear.UWP.Http.AsyncHttpClient();
            httpclient.Url("http://mobilecdn.kugou.com/api/v3/album/song?pagesize=-1&plat=0&page=1&version=8150&albumid=" + albumid);
            var json = (await httpclient.Get()).GetString();
            json = json.Replace("320hash", "hash320");
            var obj = Windows.Data.Json.JsonObject.Parse(json);
            var data = Class.data.DataContractJsonDeSerialize<List<SongData>>(obj.GetNamedObject("data").GetNamedArray("info").ToString());
            foreach (var item in data)
            {
                item.filename = item.filename.Replace(" ", "");
                item.title = item.filename.Split('-')[1];
                item.singername = item.filename.Split('-')[0];
                if(item.mvhash=="")
                {
                    item.hasmv = "Collapsed";
                }
                else
                {
                    item.hasmv = "Visible";
                }
            }
            return data;
        }

        public async void GetSingerInfo(string singerid)
        {
            var httpclient = new Noear.UWP.Http.AsyncHttpClient();
            httpclient.Url("http://mobilecdn.kugou.com/api/v3/singer/info?singerid=" + singerid);
            var json = (await httpclient.Get()).GetString();
            json = json.Replace("{size}", "150");
            var obj = Windows.Data.Json.JsonObject.Parse(json);
            singerimg.ImageSource = new Windows.UI.Xaml.Media.Imaging.BitmapImage() { UriSource = new Uri(obj.GetNamedObject("data").GetNamedString("imgurl")) };
        }

        public class Album_Info
        {
            public string albumname { get; set; }
            public string singerid { get; set; }
            public string singername { get; set; }
            public string intro { get; set; }
            public string imgurl { get; set; }
            public string publishtime { get; set; }
        }

        public class SongData:Class.Model.ISong
        {
            public string filename { get; set; }
            public string title { get; set; }
            public string singername { get; set; }
            public string hash { get; set; }
            public string mvhash { get; set; }
            public string hasmv { get; set; }
            public string sqhash { get; set; }
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
            public async Task<Class.Model.Player.NowPlay> GetNowPlay()
            {
                var music = new Class.Model.Player.NowPlay();
                music.title = filename;
                music.url = await GetUrl();
                music.hash = hash;
                music.albumid = "";
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
                    await new MessageDialog(ResourceLoader.GetForCurrentView().GetString("PlayFalied")).ShowAsync();
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

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
