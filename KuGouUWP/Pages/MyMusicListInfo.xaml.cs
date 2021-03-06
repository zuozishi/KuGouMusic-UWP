﻿using System;
using System.Collections.Generic;
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
            Title.Text = data[2];
            LoadData();
        }

        private async void LoadData()
        {
            LoadProess.IsActive = true;
            var hashlist = await GetHashList();
            if(hashlist!=null&&hashlist!="")
            {
                var key = Class.MD5.GetMd5String("2022000kgyzone");
                var httpclient = new System.Net.Http.HttpClient();
                var json =await httpclient.GetStringAsync("http://y.service.kugou.com/song/qulity?pid=20&ver=2200&mid=0&key=" + key + "&hashlist=" + hashlist);
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
            if(list.SelectedItem!=null)
            {
                var song = list.SelectedItem as MusicData;
                await song.AddToPlayList(true);
                list.SelectedIndex = -1;
            }
            LoadProess.IsActive = false;
        }

        private async Task<string> GetHashList()
        {
            try
            {
                TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
                var time = Convert.ToInt64(ts.TotalSeconds).ToString();
                var key = Class.MD5.GetMd5String("100010TKbNapLfhKd89VxM" + uid + rid + time + "netfavorite");
                var httpclient = new System.Net.Http.HttpClient();
                var json = await httpclient.GetStringAsync("http://wp.cloudlist.service.kugou.com/song/getfile?uid=366079534&appid=100010&rid=" + rid + "&ts=" + time + "&key=" + key);
                var obj = Windows.Data.Json.JsonObject.Parse(json);
                hashdata = Class.data.DataContractJsonDeSerialize<List<HashData>>(obj.GetNamedObject("data").GetNamedArray("File").ToString());
                string result = "";
                for (int i = 0; i < hashdata.Count; i++)
                {
                    if (i == hashdata.Count - 1)
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
            catch (Exception)
            {
                return null;
            }
        }

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        public static async Task<Class.Model.SearchResultModel.Singer> GetSingerResult(string singername)
        {
            try
            {
                var httpclient = new System.Net.Http.HttpClient();
                var jsondata = await httpclient.GetStringAsync("http://mobilecdn.kugou.com/api/v3/singer/info?singername=" + singername);
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
        public class MusicData:Class.Model.ISong
        {
            public HashData moredata { get; set; }
            public string hash { get; set; }
            public string hash_sq { get; set; }
            public string mvhash { get; set; }
            public string hash_320 { get; set; }
            public async Task<string> GetUrl()
            {
                try
                {
                    if (hash != "")
                    {
                        switch (Class.Setting.Qu.GetType())
                        {
                            case Class.Setting.Qu.Type.low:
                                return await Class.kugou.get_musicurl_by_hash(hash);
                            case Class.Setting.Qu.Type.mid:
                                if (hash_320 != "")
                                {
                                    hash = hash_320;
                                    return await Class.kugou.get_musicurl_by_hash(hash_320);
                                }
                                else
                                {
                                    return await Class.kugou.get_musicurl_by_hash(hash);
                                }
                            case Class.Setting.Qu.Type.high:
                                if (hash_sq != null)
                                {
                                    hash = hash_sq;
                                    return await Class.kugou.get_musicurl_by_hash(hash_sq);
                                }
                                else
                                {
                                    if (hash_320 != "")
                                    {
                                        hash = hash_320;
                                        return await Class.kugou.get_musicurl_by_hash(hash_320);
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
                        if (moredata.hash != "")
                        {
                            return await Class.kugou.get_musicurl_by_hash(moredata.hash);
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
            public async Task<Model.Player.NowPlay> GetNowPlay()
            {
                var music = new Class.Model.Player.NowPlay();
                if (moredata.filename.Contains("-"))
                {
                    var spits = moredata.filename.Split('-');
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
                    music.title = moredata.filename;
                    music.singername = "未知歌手";
                    music.imgurl = "ms-appx:///Assets/image/songimg.png";
                }
                music.url = await GetUrl();
                music.hash = hash;
                music.albumid = "";
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
                    await KG_ClassLibrary.BackgroundDownload.Start(moredata.filename, url, KG_ClassLibrary.BackgroundDownload.DownloadType.song);
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
                            if (hash_320 != "")
                            {
                                return await Class.kugou.get_musicurl_by_hash(hash_320);
                            }
                            else
                            {
                                return await Class.kugou.get_musicurl_by_hash(hash);
                            }
                        case Class.Setting.DownQu.Type.high:
                            if (hash_sq != null)
                            {
                                return await Class.kugou.get_musicurl_by_hash(hash_sq);
                            }
                            else
                            {
                                if (hash_320 != "")
                                {
                                    return await Class.kugou.get_musicurl_by_hash(hash_320);
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

        private async void SongListAdd_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            var songdata = btn.DataContext as MusicData;
            if (songdata != null)
            {
                LoadProess.IsActive = true;
                await songdata.AddToPlayList(false);
                LoadProess.IsActive = false;
            }
        }
    }
}
