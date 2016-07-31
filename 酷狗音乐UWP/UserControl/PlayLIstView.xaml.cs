using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 酷狗音乐UWP.Class;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace 酷狗音乐UWP.UserControlClass
{
    public sealed partial class PlayLIstView : UserControl
    {
        public List<ListData> listdata { get; set; }
        public Frame mainFrame { get; set; }
        public Model.PlayList list { get; private set; }

        public PlayLIstView()
        {
            this.InitializeComponent();
            mainFrame = Window.Current.Content as Frame;
            init();
        }

        public async void init()
        {
            try
            {
                if(listdata != null)
                {
                    listdata.Clear();
                }
                SongListView.Items.Clear();
                SongListView.ItemsSource = null;
            }
            catch (Exception)
            {
                
            }
            try
            {
                SongListView.ItemClick -= SongListView_ItemClick;
            }
            catch (Exception)
            {
                
            }
            Class.MediaControl.GetCurrent().CurrentStateChanged += MediaCurrentChanged;
            list = await Class.Model.PlayList.GetPlayList();
            if(list!=null)
            {
                switch (list.cyc)
                {
                    case Class.Model.PlayList.cycling.单曲循环:
                        Cyc_Btn.Icon = new SymbolIcon(Symbol.RepeatOne);
                        break;
                    case Class.Model.PlayList.cycling.列表循环:
                        Cyc_Btn.Icon = new SymbolIcon(Symbol.RepeatAll);
                        break;
                    case Class.Model.PlayList.cycling.随机播放:
                        Cyc_Btn.Icon = new SymbolIcon(Symbol.Shuffle);
                        break;
                    default:
                        break;
                }
                if (list.SongList != null)
                {
                    if(list.SongList.Count>0)
                    {
                        listdata = new List<ListData>();
                        listdata.Clear();
                        SongListView.Items.Clear();
                        SongListView.ItemsSource = null;
                        for (int i = 0; i < list.SongList.Count; i++)
                        {
                            var data = new ListData() { singername = list.SongList[i].singername, songname = list.SongList[i].title ,num=(i+1).ToString()};
                            if (list.nowplay == i)
                            {
                                data.isplay = "Visible";
                            }
                            else
                            {
                                data.isplay = "Collapsed";
                            }
                            listdata.Add(data);
                            SongListView.Items.Add(data);
                        }
                        SongListView.SelectedIndex = list.nowplay;
                        SongListView.IsItemClickEnabled = true;
                        SongListView.ItemClick += SongListView_ItemClick;
                    }
                }
            }
        }

        private async void SongListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as ListData;
            await Class.Model.PlayList.PlayAt(int.Parse(data.num) -1);
            init();
        }

        private void MediaCurrentChanged(MediaPlayer sender, object args)
        {
            switch (sender.CurrentState)
            {
                case MediaPlayerState.Playing:
                    mainFrame.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        init();
                    });
                    break;
                default:
                    break;
            }
        }

        public class ListData
        {
            public string songname { get; set; }
            public string singername { get; set; }
            public string isplay { get; set; }
            public string num { get; set; }
        }

        private async void Cyc_Btn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            var icon = btn.Icon as SymbolIcon;
            switch (icon.Symbol)
            {
                case Symbol.RepeatOne:
                    await Class.Model.PlayList.SetCycling(Class.Model.PlayList.cycling.列表循环);
                    Cyc_Btn.Icon = new SymbolIcon(Symbol.RepeatAll);
                    break;
                case Symbol.RepeatAll:
                    await Class.Model.PlayList.SetCycling(Class.Model.PlayList.cycling.随机播放);
                    Cyc_Btn.Icon = new SymbolIcon(Symbol.Shuffle);
                    break;
                case Symbol.Shuffle:
                    await Class.Model.PlayList.SetCycling(Class.Model.PlayList.cycling.单曲循环);
                    Cyc_Btn.Icon = new SymbolIcon(Symbol.RepeatOne);
                    break;
                default: break;
            }
        }

        private async void MoreBtnClicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            if (btn.DataContext != null)
            {
                var data = btn.DataContext as ListData;
                var song = list.SongList[int.Parse(data.num) - 1];
                if (btn.TabIndex==0)
                {

                }
                else if(btn.TabIndex == 1)
                {
                    LoadProgress.Visibility = Visibility.Visible;
                    if (song.url != null && song.url.Contains("http://"))
                    {
                        await KG_ClassLibrary.BackgroundDownload.Start(song.singername + "-" + song.title, song.url, KG_ClassLibrary.BackgroundDownload.DownloadType.song);
                        await new Windows.UI.Popups.MessageDialog("已加入下载列表！").ShowAsync();
                    } else
                    {
                        await new Windows.UI.Popups.MessageDialog("本地歌曲无需下载" + song.url).ShowAsync();
                    }
                    LoadProgress.Visibility = Visibility.Collapsed;
                }
                else if(btn.TabIndex == 2)
                {
                    LoadProgress.Visibility = Visibility.Visible;
                    SongListView.Items.RemoveAt(int.Parse(data.num) - 1);
                    await Class.Model.PlayList.Del(int.Parse(data.num) - 1);
                    LoadProgress.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadProgress.Visibility = Visibility.Visible;
            SongListView.Items.Clear();
            await Class.Model.PlayList.Clear();
            LoadProgress.Visibility = Visibility.Collapsed;
        }
    }
}
