using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace KuGouUWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class Search : Page
    {
        private ObservableCollection<string> history;

        public Search()
        {
            this.InitializeComponent();
            init();
        }

        private void init()
        {
            init_hotsearct();
            LoadHistory();
        }

        private async void LoadHistory()
        {
            history = await Class.Model.History.GetSearch();
            if (history != null)
            {
                SearchList.ItemsSource = history;
                SearchList.SelectionMode = ListViewSelectionMode.Single;
                SearchList.SelectionChanged += SearchList_SelectionChanged;
            }
        }

        private void SearchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedItem != null)
            {
                Frame.Navigate(typeof(Pages.SearchResult), list.SelectedItem.ToString());
            }
        }

        private async void init_hotsearct()
        {
            var httpclient=new System.Net.Http.HttpClient();
            var jsondata= await httpclient.GetStringAsync("http://mobilecdn.kugou.com/api/v3/search/hot?count=8&plat=0");
            var obj = Windows.Data.Json.JsonObject.Parse(jsondata);
            var arryobj = obj.GetNamedObject("data").GetNamedArray("info");
            for (int i = 0; i < 8; i++)
            {
                var item = (GridViewItem)(HotSearchView.Items[i]);
                var button = (Button)item.Content;
                button.Content= arryobj[i].GetObject().GetNamedString("keyword");
            }
            HotSearchView.Visibility = Visibility.Visible;
            var result = SearchBox.Focus(FocusState.Pointer);
        }

        private async void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            try
            {
                if (sender.Text.Length != 0)
                {
                    //ObservableCollection<String>
                    var data = new ObservableCollection<string>();
                    var httpclient = new System.Net.Http.HttpClient();
                    var jsondata= await httpclient.GetStringAsync("http://mobilecdn.kugou.com/new/app/i/search.php?cmd=302&keyword="+sender.Text);
                    var obj = Windows.Data.Json.JsonObject.Parse(jsondata);
                    var arryobj = obj.GetNamedArray("data");
                    foreach (var item in arryobj)
                    {
                        data.Add(item.GetObject().GetNamedString("keyword"));
                    }
                    sender.ItemsSource = data;
                    //sender.IsSuggestionListOpen = true;
                }
                else
                {
                    sender.IsSuggestionListOpen = false;
                }
            }
            catch (Exception)
            {

            }
        }

        private void Back_Btn_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void SearchSubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (sender.Text.Length > 0)
            {
                Class.Model.History.Add(sender.Text);
                await Task.Delay(100);
                Frame.Navigate(typeof(Pages.SearchResult), sender.Text);
            }
        }

        private void HotSearchBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Content.ToString().Length > 0)
            {
                Frame.Navigate(typeof(Pages.SearchResult), btn.Content.ToString());
            }
        }

        private void HistoryRemove(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var data = btn.DataContext.ToString();
            if (history != null)
            {
                for (int i = 0; i < history.Count; i++)
                {
                    if (history[i] == data)
                    {
                        history.RemoveAt(i);
                    }
                }
                Class.Model.History.Remove(data);
            }
        }
    }
}
