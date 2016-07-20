using System;
using System.Collections.Generic;
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
using 酷狗音乐UWP.Class;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace 酷狗音乐UWP.page
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MyMusicListInfo : Page
    {
        private string uid;
        private string rid;
        private List<HashData> hashdata;

        public MyMusicListInfo()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var data = e.Parameter as string[];
            uid = data[0];
            rid = data[1];
            LoadData();
        }

        private async void LoadData()
        {
            LoadProess.IsActive = true;
            var hashlist = await GetHashList();
            if(hashlist!="")
            {
                var key = Class.MD5.GetMd5String("2022000kgyzone");
                var httpclient = new Noear.UWP.Http.AsyncHttpClient();
                httpclient.Url("http://y.service.kugou.com/song/qulity?pid=20&ver=2200&mid=0&key=" + key + "&hashlist=" + hashlist);
                var json = (await httpclient.Get()).GetString();
                var obj = Windows.Data.Json.JsonObject.Parse(json);
                var musicdata = Class.data.DataContractJsonDeSerialize<List<MusicData>>(obj.GetNamedArray("list").ToString());
                for (int i = 0; i < musicdata.Count; i++)
                {
                    musicdata[i].moredata = hashdata[i];
                }
                MusicList.ItemsSource = musicdata;
                MusicList.SelectionChanged += MusicList_SelectionChanged;
            }
            LoadProess.IsActive = false;
        }

        private async void MusicList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProess.IsActive = false;
            var list = sender as ListView;
            var song = list.SelectedItem as MusicData;
            string url = "";
            if (song.hash_sq != "")
            {
                url = await kugou.get_song_url(song.hash_sq);
            }
            else
            {
                if (song.hash_320 != "")
                {
                    url = await kugou.get_song_url(song.hash_320);
                }
                else
                {
                    if (song.hash != "")
                    {
                        url = await kugou.get_song_url(song.hash);
                    }
                }
            }
            if (url != "")
            {
                var music = new Class.Model.Player.NowPlay();
                var filename = song.moredata.filename;
                if (filename.Contains("-"))
                {
                    var spits = filename.Split('-');
                    music.title = spits[1].Replace(" ", "");
                    music.singername = spits[0].Replace(" ", "");
                    var singer = await GetSingerResult(music.singername);
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
                    music.title = song.moredata.filename;
                    music.singername = "未知歌手";
                    music.imgurl = "ms-appx:///Assets/image/songimg.png";
                }
                music.url = url;
                music.albumid = "";
                await Class.Model.Player.SetNowPlay(music);
                Class.Model.PlayList.Add(music, true);
            }
            LoadProess.IsActive = false;
        }

        private async Task<string> GetHashList()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var time = Convert.ToInt64(ts.TotalSeconds).ToString();
            var key = Class.MD5.GetMd5String("100010TKbNapLfhKd89VxM" + uid + rid + time + "netfavorite");
            var httpclient = new Noear.UWP.Http.AsyncHttpClient();
            httpclient.Url("http://wp.cloudlist.service.kugou.com/song/getfile?uid=366079534&appid=100010&rid="+rid+"&ts="+time+"&key="+key);
            var json = (await httpclient.Get()).GetString();
            var obj = Windows.Data.Json.JsonObject.Parse(json);
            hashdata = Class.data.DataContractJsonDeSerialize<List<HashData>>(obj.GetNamedObject("data").GetNamedArray("File").ToString());
            string result = "";
            for (int i = 0; i < hashdata.Count; i++)
            {
                if(i==hashdata.Count-1)
                {
                    result = result + hashdata[i].hash;
                }
                else
                {
                    result = result + hashdata[i].hash + ",";
                }
            }
            return result;
        }

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        public static async Task<Class.Model.SearchResultModel.Singer> GetSingerResult(string singername)
        {
            try
            {
                var httpclient = new Noear.UWP.Http.AsyncHttpClient();
                httpclient.Url("http://mobilecdn.kugou.com/api/v3/singer/info?singername=" + singername);
                var httpresult = await httpclient.Get();
                var jsondata = httpresult.GetString();
                jsondata = jsondata.Replace("{size}", "150");
                var obj = Windows.Data.Json.JsonObject.Parse(jsondata).GetNamedObject("data").ToString();
                var resultdata = Class.data.DataContractJsonDeSerialize<Model.SearchResultModel.Singer>(obj);
                return resultdata;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public class HashData
        {
            public string filename { get; set; }
            public string hash { get; set; }
        }
        public class MusicData
        {
            public HashData moredata { get; set; }
            public string hash { get; set; }
            public string hash_sq { get; set; }
            public string mvhash { get; set; }
            public string hash_320 { get; set; }
        }
    }
}
