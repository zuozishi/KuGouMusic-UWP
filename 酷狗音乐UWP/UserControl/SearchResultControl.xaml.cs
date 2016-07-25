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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 酷狗音乐UWP.Class;
using static 酷狗音乐UWP.Class.Model.SearchResultModel;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace 酷狗音乐UWP.UserControlClass
{
    public sealed partial class SearchResultControl : UserControl
    {
        bool isfirst = true;
        int song_page = 1, mv_page = 1, songlistpage = 1, album_page = 1, lrc_page = 1;
        public string keyword { get; set; }
        public ObservableCollection<Model.SearchResultModel.Song> MusicData { get; private set; }
        public ObservableCollection<Model.SearchResultModel.MV> MVData { get; private set; }
        public ObservableCollection<Model.SearchResultModel.Album> AlbumData { get; private set; }
        public ObservableCollection<Model.SearchResultModel.SongList> SonglistData { get; private set; }
        public ObservableCollection<Model.SearchResultModel.Lrc> LrcData { get; private set; }
        public Frame mainframe;
        private object o = new object();
        private Singer singerdata;

        public SearchResultControl()
        {
            this.InitializeComponent();
        }

        public async void StartSearch(Frame frame,string key)
        {
            mainframe = frame;
            if (keyword == key)
            {
                return;
            }
            keyword = key;
            song_page = 1; mv_page = 1; songlistpage = 1; album_page = 1; lrc_page = 1;
            MusicData = null;
            MVData = null;
            AlbumData = null;
            AlbumData = null;
            SonglistData = null;
            LrcData = null;
            try
            {
                SongResultList.Items.Clear();
                MVResultList.Items.Clear();
                AlbumResultList.Items.Clear();
                SonglistResultList.Items.Clear();
                LrcResultList.Items.Clear();
            }
            catch (Exception)
            {
                
            }
            var thispage = ResultView.SelectedIndex;
            await LoadSinger();
            await LoadData(thispage);
        }
        public async Task LoadSinger()
        {
            singerdata = await GetSingerResult(keyword);
            if (singerdata!=null&&singerdata.pics!=null)
            {
                SingerName_Text.Text = singerdata.singername;
                Singer_Image.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage() { UriSource=new Uri(singerdata.imgurl)};
                SingerInfo_Text.Text = String.Format("歌曲 {0}/ 专辑 {1}/MV {2}",singerdata.songcount,singerdata.albumcount,singerdata.mvcount);
                SingerGrid.Visibility = Visibility.Visible;
                SongScrollViewer.Margin = new Thickness() { Top=100};
                MoreSongsContorl.Margin = new Thickness() { Top = 70 };
            }
            else
            {
                SingerGrid.Visibility = Visibility.Collapsed;
                SongScrollViewer.Margin = new Thickness() { Top = 30 };
                MoreSongsContorl.Margin = new Thickness();
            }
        }
        public async Task LoadData(int page_index)
        {
            switch (page_index)
            {
                case 0:
                    if (MusicData==null)
                    {
                        SongLoadProgress.IsActive = true;
                        MusicData = await GetSongResult(keyword, song_page);
                        if(MusicData!=null)
                        {
                            foreach (var item in MusicData)
                            {
                                SongResultList.Items.Add(item);
                            }
                        }
                        SongLoadProgress.IsActive = false;
                    }
                    break;
                case 1:
                    if (MVData == null)
                    {
                        MVLoadProgress.IsActive = true;
                        MVData = await GetMVResult(keyword, mv_page);
                        if (MVData != null)
                        {
                            foreach (var item in MVData)
                            {
                                MVResultList.Items.Add(item);
                            }
                        }
                        MVLoadProgress.IsActive = false;
                    }
                    break;
                case 2:
                    if (AlbumData == null)
                    {
                        AlbumLoadProgress.IsActive = true;
                        AlbumData = await GetAlbumResult(keyword, album_page);
                        if (AlbumData != null)
                        {
                            foreach (var item in AlbumData)
                            {
                                AlbumResultList.Items.Add(item);
                            }
                        }
                        AlbumLoadProgress.IsActive = false;
                    }
                    break;
                case 3:
                    if (SonglistData == null)
                    {
                        SonglistLoadProgress.IsActive = true;
                        SonglistData = await GetSonglistResult(keyword, songlistpage);
                        if(SonglistData!=null)
                        {
                            SonglistResultList.Items.Clear();
                            foreach (var item in SonglistData)
                            {
                                SonglistResultList.Items.Add(item);
                            }
                        }
                        SonglistLoadProgress.IsActive = false;
                    }
                    break;
                case 4:
                    if (LrcData == null)
                    {
                        LrcLoadProgress.IsActive = true;
                        LrcData = await GetLrcResult(keyword, lrc_page);
                        if(LrcData!=null)
                        {
                            foreach (var item in LrcData)
                            {
                                LrcResultList.Items.Add(item);
                            }
                        }
                        LrcLoadProgress.IsActive = false;
                    }
                    break;
                default:
                    break;
            }
        }

        private void ResultView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (isfirst)
                {
                    isfirst = false;
                    return;
                }
                var Theme = (Application.Current.Resources.ThemeDictionaries.ToList())[0].Value as ResourceDictionary;
                switch (ResultView.SelectedIndex)
                {
                    case 0:
                        Song_Btn.BorderBrush = (SolidColorBrush)Theme["KuGou-Foreground"];
                        MV_Btn.BorderBrush = null;
                        Album_Btn.BorderBrush = null;
                        SongList_Btn.BorderBrush = null;
                        Lrc_Btn.BorderBrush = null;
                        break;
                    case 1:
                        Song_Btn.BorderBrush = null;
                        MV_Btn.BorderBrush = (SolidColorBrush)Theme["KuGou-Foreground"];
                        Album_Btn.BorderBrush = null;
                        SongList_Btn.BorderBrush = null;
                        Lrc_Btn.BorderBrush = null;
                        break;
                    case 2:
                        Song_Btn.BorderBrush = null; ;
                        MV_Btn.BorderBrush = null;
                        Album_Btn.BorderBrush = (SolidColorBrush)Theme["KuGou-Foreground"];
                        SongList_Btn.BorderBrush = null;
                        Lrc_Btn.BorderBrush = null;
                        break;
                    case 3:
                        Song_Btn.BorderBrush = null;
                        MV_Btn.BorderBrush = null;
                        Album_Btn.BorderBrush = null;
                        SongList_Btn.BorderBrush = (SolidColorBrush)Theme["KuGou-Foreground"];
                        Lrc_Btn.BorderBrush = null;
                        break;
                    case 4:
                        Song_Btn.BorderBrush = null;
                        MV_Btn.BorderBrush = null;
                        Album_Btn.BorderBrush = null;
                        SongList_Btn.BorderBrush = null;
                        Lrc_Btn.BorderBrush = (SolidColorBrush)Theme["KuGou-Foreground"];
                        break;
                    default:
                        break;
                }
                LoadData(ResultView.SelectedIndex);
            }
            catch (Exception)
            {
                
            }
        }

        private void ListView_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

        }

        private void ListView_PointerExited(object sender, PointerRoutedEventArgs e)
        {

        }

        public static async Task<ObservableCollection<Class.Model.SearchResultModel.Song>> GetSongResult(string keyword, int page)
        {
            try
            {
                var httpclient = new Noear.UWP.Http.AsyncHttpClient();
                httpclient.Url("http://mobilecdn.kugou.com/api/v3/search/song?pagesize=20&sver=2&page=" + page + "&version=8150&keyword=" + keyword);
                var httpresult = await httpclient.Get();
                var jsondata = httpresult.GetString();
                jsondata = jsondata.Replace("320hash", "hash320");
                var obj = Windows.Data.Json.JsonObject.Parse(jsondata);
                var arry = obj.GetNamedObject("data").GetNamedArray("info");
                var resultdata = new ObservableCollection<Class.Model.SearchResultModel.Song>();
                foreach (var item in arry)
                {
                    var music = Class.data.DataContractJsonDeSerialize<Class.Model.SearchResultModel.Song>(item.ToString());
                    music.filename = music.filename.Replace("-", "");
                    if (music.singername!=null)
                    {
                        music.filename = music.filename.Replace(music.singername, "");
                    }
                    music.filename = music.filename.Replace(" ", "");
                    if(music.filename.Contains("【"))
                    {
                        music.filename = music.filename.Split('【')[0];
                    }
                    if(music.mvhash.Length>0)
                    {
                        music.mvview = "Visible";
                    }
                    else
                    {
                        music.mvview = "Collapsed";
                    }
                    resultdata.Add(music);
                }
                return resultdata;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void TopBtnClicked(object sender, RoutedEventArgs e)
        {
            var btn=sender as Button;
            ResultView.SelectedIndex = btn.TabIndex;
            switch (btn.TabIndex)
            {
                case 0:
                    Song_Btn.BorderBrush = new SolidColorBrush(Colors.White);
                    MV_Btn.BorderBrush = null;
                    Album_Btn.BorderBrush = null;
                    SongList_Btn.BorderBrush = null;
                    Lrc_Btn.BorderBrush = null;
                    break;
                case 1:
                    Song_Btn.BorderBrush = null;
                    MV_Btn.BorderBrush = new SolidColorBrush(Colors.White);
                    Album_Btn.BorderBrush = null;
                    SongList_Btn.BorderBrush = null;
                    Lrc_Btn.BorderBrush = null;
                    break;
                case 2:
                    Song_Btn.BorderBrush = null; ;
                    MV_Btn.BorderBrush = null;
                    Album_Btn.BorderBrush = new SolidColorBrush(Colors.White);
                    SongList_Btn.BorderBrush = null;
                    Lrc_Btn.BorderBrush = null;
                    break;
                case 3:
                    Song_Btn.BorderBrush = null;
                    MV_Btn.BorderBrush = null;
                    Album_Btn.BorderBrush = null;
                    SongList_Btn.BorderBrush = new SolidColorBrush(Colors.White);
                    Lrc_Btn.BorderBrush = null;
                    break;
                case 4:
                    Song_Btn.BorderBrush = null;
                    MV_Btn.BorderBrush = null;
                    Album_Btn.BorderBrush = null;
                    SongList_Btn.BorderBrush = null;
                    Lrc_Btn.BorderBrush = new SolidColorBrush(Colors.White);
                    break;
                default:
                    break;
            }
        }

        public static async Task<ObservableCollection<Class.Model.SearchResultModel.MV>> GetMVResult(string keyword, int page)
        {
            try
            {
                var httpclient = new Noear.UWP.Http.AsyncHttpClient();
                httpclient.Url("http://mobilecdn.kugou.com/api/v3/search/mv?pagesize=20&sver=2&page=" + page + "&version=8150&keyword=" + keyword);
                var httpresult = await httpclient.Get();
                var jsondata = httpresult.GetString();
                jsondata = jsondata.Replace("{size}", "150");
                var obj = Windows.Data.Json.JsonObject.Parse(jsondata);
                var arry = obj.GetNamedObject("data").GetNamedArray("info");
                var resultdata = new ObservableCollection<Class.Model.SearchResultModel.MV>();
                foreach (var item in arry)
                {
                    resultdata.Add(Class.data.DataContractJsonDeSerialize<Class.Model.SearchResultModel.MV>(item.ToString()));
                }
                return resultdata;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async void LoadMoreBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var thispage = ResultView.SelectedIndex;
            MusicData = null;
            MVData = null;
            AlbumData = null;
            SonglistData = null;
            LrcData = null;
            switch (thispage)
            {
                case 0:
                    if (MusicData == null)
                    {
                        SongLoadProgress.IsActive = true;
                        MusicData = await GetSongResult(keyword, song_page);
                        foreach (var item in MusicData)
                        {
                            SongResultList.Items.Add(item);
                        }
                        //SongResultList.ItemsSource = MusicData;
                        SongLoadProgress.IsActive = false;
                    }
                    break;
                case 1:
                    if (MVData == null)
                    {
                        MVLoadProgress.IsActive = true;
                        MVData = await GetMVResult(keyword, mv_page);
                        MVResultList.ItemsSource = MVData;
                        MVLoadProgress.IsActive = false;
                    }
                    break;
                case 2:
                    if (AlbumData == null)
                    {
                        AlbumLoadProgress.IsActive = true;
                        AlbumData = await GetAlbumResult(keyword, album_page);
                        foreach (var item in AlbumData)
                        {
                            AlbumResultList.Items.Add(item);
                        }
                        //AlbumResultList.ItemsSource = AlbumData;
                        AlbumLoadProgress.IsActive = false;
                    }
                    break;
                case 3:
                    if (SonglistData == null)
                    {
                        SonglistLoadProgress.IsActive = true;
                        SonglistData = await GetSonglistResult(keyword, songlistpage);
                        SonglistResultList.ItemsSource = SonglistData;
                        SonglistLoadProgress.IsActive = false;
                    }
                    break;
                case 4:
                    if (LrcData == null)
                    {
                        LrcLoadProgress.IsActive = true;
                        LrcData = await GetLrcResult(keyword, lrc_page);
                        LrcResultList.ItemsSource = LrcData;
                        LrcLoadProgress.IsActive = false;
                    }
                    break;
                default:
                    break;
            }
        }

        private void LoadMore_Changed(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv_SP = sender as ScrollViewer;
            if (sv_SP.VerticalOffset == sv_SP.ScrollableHeight)
            {
                var thispage = ResultView.SelectedIndex;
                switch (thispage)
                {
                    case 0:
                        lock (o)
                        {
                            if (!SongLoadProgress.IsActive)
                            {
                                SongLoadProgress.IsActive = true;
                                song_page = song_page + 1;
                                Task.Factory.StartNew(async () =>
                                {
                                    var data = await GetSongResult(keyword, song_page);
                                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                    {
                                        foreach (var item in data)
                                        {
                                            SongResultList.Items.Add(item);
                                        }
                                        SongLoadProgress.IsActive = false;
                                    });
                                });
                            }
                        }
                        break;
                    case 1:
                        lock (o)
                        {
                            if (!MVLoadProgress.IsActive)
                            {
                                MVLoadProgress.IsActive = true;
                                mv_page = mv_page + 1;
                                Task.Factory.StartNew(async () =>
                                {
                                    var data = await GetMVResult(keyword, mv_page);
                                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                    {
                                        foreach (var item in data)
                                        {
                                            MVResultList.Items.Add(item);
                                        }
                                        MVLoadProgress.IsActive = false;
                                    });
                                });
                            }
                        }
                        break;
                    case 2:
                        lock (o)
                        {
                            if (!AlbumLoadProgress.IsActive)
                            {
                                AlbumLoadProgress.IsActive = true;
                                album_page = album_page + 1;
                                Task.Factory.StartNew(async () =>
                                {
                                    var data = await GetAlbumResult(keyword, album_page);
                                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                    {
                                        foreach (var item in data)
                                        {
                                            AlbumResultList.Items.Add(item);
                                        }
                                        AlbumLoadProgress.IsActive = false;
                                    });
                                });
                            }
                        }
                        break;
                    case 3:
                        lock (o)
                        {
                            if (!SonglistLoadProgress.IsActive)
                            {
                                SonglistLoadProgress.IsActive = true;
                                songlistpage = songlistpage + 1;
                                Task.Factory.StartNew(async () =>
                                {
                                    var data = await GetSonglistResult(keyword, songlistpage);
                                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                    {
                                        foreach (var item in data)
                                        {
                                            SonglistResultList.Items.Add(item);
                                        }
                                        SonglistLoadProgress.IsActive = false;
                                    });
                                });
                            }
                        }
                        break;
                    case 4:
                        lock (o)
                        {
                            if (!LrcLoadProgress.IsActive)
                            {
                                LrcLoadProgress.IsActive = true;
                                lrc_page = lrc_page + 1;
                                Task.Factory.StartNew(async () =>
                                {
                                    var data = await GetLrcResult(keyword, lrc_page);
                                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                    {
                                        foreach (var item in data)
                                        {
                                            LrcResultList.Items.Add(item);
                                        }
                                        LrcLoadProgress.IsActive = false;
                                    });
                                });
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void SingerGrid_Click(object sender, RoutedEventArgs e)
        {
            if (singerdata != null)
            {
                var frame = Window.Current.Content as Frame;
                frame.Navigate(typeof(page.SingerPage), singerdata.singerid);
            }
        }

        private void MoreSongsContorl_Click(object sender, RoutedEventArgs e)
        {
            if (SongResultList.SelectionMode == ListViewSelectionMode.Single)
            {
                SongResultList.SelectionMode = ListViewSelectionMode.Multiple;
                MoreSongBox.Show();
                MoreSongBox.BtnClickedEvent += MoreSongBox_BtnClickedEvent;
            }
            else
            {
                SongResultList.SelectionMode = ListViewSelectionMode.Single;
                MoreSongBox.Hidden();
            }
        }

        private async void MoreSongBox_BtnClickedEvent(SongMultipleBox.BtnType type)
        {
            if(SongResultList.SelectedItems!=null&& SongResultList.SelectedItems.Count > 0)
            {
                switch (type)
                {
                    case SongMultipleBox.BtnType.NextPlay:
                        foreach (var item in SongResultList.SelectedItems)
                        {
                            var song = item as Model.SearchResultModel.Song;
                            await song.AddToPlayList(false);
                        }
                        break;
                    case SongMultipleBox.BtnType.Download:
                        foreach (var item in SongResultList.SelectedItems)
                        {
                            var song = item as Model.SearchResultModel.Song;
                            await song.AddToDownloadList();
                        }
                        break;
                    case SongMultipleBox.BtnType.AddToList:
                        break;
                    default:
                        break;
                }
            }
        }

        private async void ListSelection_Changes(object sender, SelectionChangedEventArgs e)
        {
            var thispage = ResultView.SelectedIndex;
            switch (thispage)
            {
                case 0:
                    if(SongResultList.SelectionMode==ListViewSelectionMode.Single)
                    {
                        SongLoadProgress.IsActive = true;
                        var obj = sender as ListView;
                        if (obj.SelectedItem != null)
                        {
                            var song = obj.SelectedItem as Model.SearchResultModel.Song;
                            await song.AddToPlayList(true);
                        }
                        obj.SelectedIndex = -1;
                        SongLoadProgress.IsActive = false;
                    }
                    break;
                case 1:
                    var obj1 = sender as ListView;
                    if (obj1.SelectedItem != null)
                    {
                        var mv = (Model.SearchResultModel.MV)obj1.SelectedItem;
                        mainframe.Navigate(typeof(page.MVPlayer), mv.hash);
                    }
                    obj1.SelectedIndex = -1;
                    break;
                case 2:
                    var obj2 = sender as GridView;
                    if (obj2.SelectedItem != null)
                    {
                        var album = (Model.SearchResultModel.Album)obj2.SelectedItem;
                        mainframe.Navigate(typeof(page.AlbumPage), album.albumid);
                    }
                    obj2.SelectedIndex = -1;
                    break;
                default:
                    break;
            }
        }

        public async Task<string> LoadLrcFromSongName(string songname, string duration)
        {
            var httpclient = new Noear.UWP.Http.AsyncHttpClient();
            httpclient.Url("http://lyrics.kugou.com/search?ver=1&man=yes&client=pc&keyword=" + songname + "&duration=" + duration+"000");
            var json = (await httpclient.Get()).GetString();
            var arry = Windows.Data.Json.JsonObject.Parse(json).GetNamedArray("candidates");
            if (arry.Count != 0)
            {
                var id = arry[0].GetObject().GetNamedString("id");
                var accesskey = arry[0].GetObject().GetNamedString("accesskey");
                var lrc=await LoadLrcFormId(id, accesskey);
                return lrc;
            }
            else
            {
                return null;
            }
        }

        public async Task<string> LoadLrcFormId(string id, string accesskey)
        {
            var httpclient = new Noear.UWP.Http.AsyncHttpClient();
            httpclient.Url("http://lyrics.kugou.com/download?ver=1&client=pc&id=" + id + "&accesskey=" + accesskey + "&fmt=lrc&charset=utf8");
            var json = (await httpclient.Get()).GetString();
            var content = Windows.Data.Json.JsonObject.Parse(json).GetNamedString("content");
            var data = Convert.FromBase64String(content);
            json = System.Text.ASCIIEncoding.UTF8.GetString(data);
            return json;
        }

        private void MoreSongsContorl_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var grid = sender as StackPanel;
            grid.Background = new SolidColorBrush();
        }
    }
}
