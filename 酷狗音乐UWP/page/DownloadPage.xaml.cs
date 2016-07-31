using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using KG_ClassLibrary;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.BackgroundTransfer;
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
    public sealed partial class DownloadPage : Page
    {
        private BackgroundDownload.ResultData songresult;
        private BackgroundDownload.ResultData mvresult;

        public ObservableCollection<DownloadManager.DownloadModel> SongDowns { get; private set; }

        public class txtdata
        {
            public string text { get; set; }
        }
        public DownloadPage()
        {
            this.InitializeComponent();
            init();
        }

        private void init()
        {
            //await KG_ClassLibrary.BackgroundDownload.Start("123444.flac", "http://fs.pc.kugou.com/201607200123/005f3d8bbfdde9b4111164787f8a622b/G001/M07/07/1F/QQ0DAFSTAB6Aebl0AcQxqCQQQSM04.flac", KG_ClassLibrary.BackgroundDownload.DownloadType.song);
            GetSongDowning();
            GetMVDowning();
        }

        private async void GetSongDowning()
        {
            //await Task.Delay(3000);
            songresult= await BackgroundDownload.GetList(BackgroundDownload.DownloadType.song);
            if(songresult!=null)
            {
                SongDowningNum.Text = songresult.transfers.Count.ToString();
                SongFileNum.Text = songresult.files.Count.ToString();
                SongDowningList.ItemsSource = songresult.transfers;
                LocalSongList.Items.Clear();
                foreach (var file in songresult.files)
                {
                    LocalSongList.Items.Add(file);
                }
            }
        }

        private async void GetMVDowning()
        {
            mvresult = await BackgroundDownload.GetList(BackgroundDownload.DownloadType.mv);
            if (mvresult != null)
            {
                MVDowningNum.Text = mvresult.transfers.Count.ToString();
                MVFileNum.Text = mvresult.files.Count.ToString();
                MVDowningList.ItemsSource = mvresult.transfers;
                LocalMVList.Items.Clear();
                foreach (var file in mvresult.files)
                {
                    LocalMVList.Items.Add(file);
                }
            }
        }

        private void TopBtnClicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            flipview.SelectedIndex = btn.TabIndex;
        }

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void flipview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var view = sender as FlipView;
            switch (view.SelectedIndex)
            {
                case 0:
                    SongList_Btn.BorderThickness = new Thickness(0, 0, 0, 2);
                    MVList_Btn.BorderThickness = new Thickness(0);
                    break;
                case 1:
                    SongList_Btn.BorderThickness = new Thickness(0);
                    MVList_Btn.BorderThickness = new Thickness(0,0,0,2);
                    break;
                default:
                    break;
            }
        }

        private async void DownItemDel(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            var downop = btn.DataContext as TransferModel;
            await downop.DownloadOperation.ResultFile.DeleteAsync();
            for (int i = 0; i < SongDowningList.Items.Count; i++)
            {
                var item = SongDowningList.Items[i] as TransferModel;
                if (item.filename == downop.filename && item.allsize == downop.allsize)
                {
                    SongDowningList.Items.RemoveAt(i);
                    break;
                }
            }
            for (int i = 0; i < MVDowningList.Items.Count; i++)
            {
                var item = MVDowningList.Items[i] as TransferModel;
                if (item.filename == downop.filename && item.allsize == downop.allsize)
                {
                    MVDowningList.Items.RemoveAt(i);
                    break;
                }
            }
        }

        private async void FileItemDel(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            var file = btn.DataContext as Windows.Storage.StorageFile;
            await file.DeleteAsync();
            for (int i = 0; i < LocalSongList.Items.Count; i++)
            {
                var item = LocalSongList.Items[i] as Windows.Storage.StorageFile;
                if (item.Path == file.Path)
                {
                    LocalSongList.Items.RemoveAt(i);
                    break;
                }
            }
            for (int i = 0; i < LocalMVList.Items.Count; i++)
            {
                var item = LocalMVList.Items[i] as Windows.Storage.StorageFile;
                if (item.Path == file.Path)
                {
                    LocalMVList.Items.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
