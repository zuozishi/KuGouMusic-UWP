using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using KuGouMusicUWP.Class;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace KuGouMusicUWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HistoryPage : Page
    {
        private ObservableCollection<Model.Player.NowPlay> songdata;
        private ObservableCollection<Model.History.HistoryMV> mvdata;

        public HistoryPage()
        {
            this.InitializeComponent();
            init();
        }

        private async void init()
        {
            songdata = await Class.Model.History.Get(new Class.Model.History.songflag());
            if (songdata != null)
            {
                songlist.ItemsSource = songdata;
                songlist.SelectionMode = ListViewSelectionMode.Single;
                songlist.SelectionChanged += Songlist_SelectionChanged;
            }
            mvdata = await Class.Model.History.Get(new Class.Model.History.mvflag());
            if (mvdata != null)
            {
                mvlist.ItemsSource = mvdata;
                mvlist.SelectionMode = ListViewSelectionMode.Single;
                mvlist.SelectionChanged += Mvlist_SelectionChanged;
            }
        }

        private void Mvlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedItem != null)
            {
                var data = list.SelectedItem as Class.Model.History.HistoryMV;
                Frame.Navigate(typeof(Pages.MVPlayer), data.hash);
                list.SelectedIndex = -1;
            }
        }

        private async void Songlist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedItem != null)
            {
                var data = list.SelectedItem as Class.Model.Player.NowPlay;
                await Class.Model.PlayList.Add(data, true);
                list.SelectedIndex = -1;
            }
        }

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void TopBtnClicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            flipview.SelectedIndex = btn.TabIndex;
        }

        private void flipview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var Theme = (Application.Current.Resources.ThemeDictionaries.ToList())[0].Value as ResourceDictionary;
            var view = sender as FlipView;
            switch (view.SelectedIndex)
            {
                case 0:
                    TopSongBtn.BorderBrush= (SolidColorBrush)Theme["KuGou-Foreground"];
                    TopMVBtn.BorderBrush = new SolidColorBrush();
                    break;
                case 1:
                    TopSongBtn.BorderBrush = new SolidColorBrush();
                    TopMVBtn.BorderBrush = (SolidColorBrush)Theme["KuGou-Foreground"];
                    break;
                default:
                    break;
            }
        }

        private void MVListItemDel(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            if (btn.DataContext != null)
            {
                var item = btn.DataContext as Class.Model.History.HistoryMV;
                mvdata.Remove(item);
                Class.Model.History.Remove(item);

            }
        }

        private void SongListItemDel(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            if (btn.DataContext != null)
            {
                var item = btn.DataContext as Class.Model.Player.NowPlay;
                songdata.Remove(item);
                Class.Model.History.Remove(item);
            }
        }
    }
}
