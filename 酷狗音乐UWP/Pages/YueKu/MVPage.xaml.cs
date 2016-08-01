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

namespace KuGouMusicUWP.Pages.YueKu
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MVPage : Page
    {
        private NewMVData newmvobj;
        private List<LibTypeInGroup> libtypeingroup;
        private Dictionary<string, string> nowlibtype;
        private MVLibData libmvdata;

        public MVPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadProcess.IsActive = true;
            var ids = e.Parameter as string;
            TuiJianFrame.LoadData();
            await LoadNewMVData();
            await LoadMVLib(ids);
            LoadProcess.IsActive = false;
        }

        private async Task LoadMVLib(string ids)
        {
            LoadProcess.IsActive = true;
            await LoadMVLibType();
            libmvdata = new MVLibData();
            if (ids != null)
            {
                flipview.SelectedIndex = 2;
                libmvdata.ids = ids;
            }
            await libmvdata.LoadData();
            mvlistView.ItemsSource = libmvdata.List;
            TuiJianFrame.TuiJianTopBtnCLicked += TuiJianFrame_TuiJianTopBtnCLicked;
            LoadProcess.IsActive = false;
        }

        private async void TuiJianFrame_TuiJianTopBtnCLicked(string ids)
        {
            LoadProcess.IsActive = true;
            flipview.SelectedIndex = 2;
            libmvdata.ids = ids;
            libmvdata.page = 0;
            libmvdata.type = MVLibData.Type.最热;
            libmvdata.List.Clear();
            await libmvdata.LoadData();
            LoadProcess.IsActive = false;
        }

        private async Task LoadMVLibType()
        {
            var httpclient = new Noear.UWP.Http.AsyncHttpClient();
            httpclient.Url("http://mobilecdn.kugou.com/api/v3/mv/taglistv2?plat=0");
            var json = (await httpclient.Get()).GetString();
            var obj = Windows.Data.Json.JsonObject.Parse(json).GetNamedObject("data");
            var data = Class.data.DataContractJsonDeSerialize<List<LibTypeData>>(obj.GetNamedArray("info").ToString());
            var menulist = new List<MenuFlyoutSubItem>();
            libtypeingroup = new List<LibTypeInGroup>();
            nowlibtype=new Dictionary<string,string>();
            foreach (var item in data)
            {
                var menu = new MenuFlyoutSubItem();
                menu.Text = item.pname;
                var allmenu1= new MenuFlyoutItem() { Text = "全部", Tag = item.pname };
                allmenu1.Click += LibTypeMenu_Click;
                menu.Items.Add(allmenu1);
                libtypeingroup.Add(new LibTypeInGroup() { pname = item.pname, name = "全部", ids = item.pname });
                foreach (var child in item.childs)
                {
                    var amenu = new MenuFlyoutItem() { Text = child.name, Tag = child.ids };
                    amenu.Click += LibTypeMenu_Click;
                    menu.Items.Add(amenu);
                    libtypeingroup.Add(new LibTypeInGroup() { pname = item.pname, name = child.name, ids = child.ids });
                }
                menulist.Add(menu);
                nowlibtype.Add(item.pname, item.pname);
            }
            var yeardata = Class.data.DataContractJsonDeSerialize<List<LibYear>>(obj.GetNamedObject("yeartag").GetObject().GetNamedArray("child").ToString());
            var yearmenus= new MenuFlyoutSubItem() { Text = "年代" };
            var allmenu = new MenuFlyoutItem() { Text = "全部", Tag = "年代" };
            nowlibtype.Add("年代", "年代");
            allmenu.Click += YearMenu_Click;
            yearmenus.Items.Add(allmenu);
            foreach (var item in yeardata)
            {
                var amenu = new MenuFlyoutItem() { Text = item.yname, Tag = item.year };
                amenu.Click += YearMenu_Click; ;
                yearmenus.Items.Add(amenu);
            }
            menulist.Add(yearmenus);
            libtypemenu.Items.Clear();
            foreach (var item in menulist)
            {
                libtypemenu.Items.Add(item);
            }
            InitLibType();
        }

        private async void YearMenu_Click(object sender, RoutedEventArgs e)
        {
            var menu = sender as MenuFlyoutItem;
            if (menu.Text != null && menu.Tag != null)
            {
                nowlibtype["年代"] = menu.Tag.ToString();
                if (libmvdata != null)
                {
                    await libmvdata.ChangeType(nowlibtype);
                }
                InitLibType();
            }
        }

        private void InitLibType()
        {
            var Theme = (Application.Current.Resources.ThemeDictionaries.ToList())[0].Value as ResourceDictionary;
            foreach (var menus in libtypemenu.Items)
            {
                foreach (var menu in ((MenuFlyoutSubItem)menus).Items)
                {
                    bool isnow = false;
                    foreach (var item in nowlibtype.Values)
                    {
                        if (menu.Tag.ToString() == item)
                        {
                            isnow = true;
                        }
                    }
                    if(isnow)
                    {
                        menu.Foreground = ((SolidColorBrush)Theme["KuGou-BackgroundColor"]);
                    }else
                    {
                        menu.Foreground = ((SolidColorBrush)Theme["KuGou-Front1"]);
                    }
                }
            }
            
        }

        private async void LibTypeMenu_Click(object sender, RoutedEventArgs e)
        {
            LoadProcess.IsActive = true;
            var menu = sender as MenuFlyoutItem;
            if (menu.Text != null && menu.Tag != null)
            {
                foreach (var item in libtypeingroup)
                {
                    if (menu.Tag.ToString() == item.ids)
                    {
                        nowlibtype[item.pname] = menu.Tag.ToString();
                        if (libmvdata != null)
                        {
                            await libmvdata.ChangeType(nowlibtype);
                        }
                        InitLibType();
                    }
                }
            }
            LoadProcess.IsActive = false;
        }

        private async Task LoadNewMVData()
        {
            newmvobj = new NewMVData();
            await newmvobj.LoadData();
            NewMVList.ItemsSource = newmvobj.List;
            NewMVList.SelectionMode = ListViewSelectionMode.Single;
            NewMVList.SelectionChanged += NewMVList_SelectionChanged;
        }

        private void NewMVList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedItem != null)
            {
                var data = list.SelectedItem as NewMVData.MVData;
                Frame.Navigate(typeof(Pages.MVPlayer), data.hash);
            }
        }

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        public class NewMVData
        {
            public ObservableCollection<MVData> List { get; set; }
            public int page = 0;
            public Type type =Type.华语;
            public NewMVData()
            {
                List = new ObservableCollection<MVData>();
            }
            public enum Type
            {
                华语=1,日韩=2,欧美=3
            }
            public class MVData
            {
                public string filename { get; set; }
                public string singername { get; set; }
                public string hash { get; set; }
                public string Content { get; set; }
                public string imgurl { get; set; }
            }
            public async Task LoadData()
            {
                page = page + 1;
                var httpclient = new Noear.UWP.Http.AsyncHttpClient();
                httpclient.Url("http://mobilecdn.kugou.com/api/v3/mv/list?type="+ (int)type + "&pagesize=20&page="+page);
                var json = (await httpclient.Get()).GetString();
                json = json.Replace("{size}", "400");
                var obj = Windows.Data.Json.JsonObject.Parse(json);
                var data = Class.data.DataContractJsonDeSerialize<ObservableCollection<MVData>>(obj.GetNamedObject("data").GetNamedArray("info").ToString());
                foreach (var item in data)
                {
                    List.Add(item);
                }
            }
            public async Task ChangeType(Type type)
            {
                this.type = type;
                page = 0;
                List.Clear();
                await LoadData();
            }
        }

        public class LibTypeData
        {
            public string pname { get; set; }
            public List<ChildData> childs { get; set; }
            public class ChildData
            {
                public string name { get; set; }
                public string ids { get; set; }
            }
        }

        public class LibYear
        {
            public string yname { get; set; }
            public string year { get; set; }
        }

        public class LibTypeInGroup
        {
            public string pname { get; set; }
            public string name { get; set; }
            public string ids { get; set; }
        }

        public class MVLibData
        {
            public int page = 0;
            public string ids = "";
            public string year = "";
            public Type type = Type.最热;
            public ObservableCollection<MVData> List { get; set; }
            public enum Type
            {
                最新=1,最热=2
            }
            public MVLibData()
            {
                List = new ObservableCollection<MVData>();
            }
            public class MVData
            {
                public string filename { get; set; }
                public string singername { get; set; }
                public string hash { get; set; }
                public string imgurl { get; set; }
                public string intro { get; set; }
            }
            public async Task LoadData()
            {
                page = page +1;
                var httpclient = new Noear.UWP.Http.AsyncHttpClient();
                httpclient.Url("http://mobilecdn.kugou.com/api/v3/mv/tagMvsV2?pagesize=20&year="+year+"&plat=0&type="+(int)type+"&page="+page+"&ids="+ids);
                var json = (await httpclient.Get()).GetString();
                json = json.Replace("{size}", "150");
                var obj = Windows.Data.Json.JsonObject.Parse(json);
                var data = Class.data.DataContractJsonDeSerialize<ObservableCollection<MVData>>(obj.GetNamedObject("data").GetNamedArray("info").ToString());
                foreach (var item in data)
                {
                    List.Add(item);
                }
            }
            public async Task ChangeType(Type sort)
            {
                this.type = sort;
                page = 0;
                List.Clear();
                await LoadData();
            }
            public async Task ChangeType(Dictionary<string,string> dir)
            {
                List.Clear();
                page = 0;
                this.ids = "";
                foreach (var item in dir)
                {
                    if (item.Key == "年代")
                    {
                        if (item.Key == item.Value)
                        {
                            this.year = "";
                        }
                        else
                        {
                            this.year = item.Value;
                        }
                    }else
                    {
                        if (item.Key != item.Value)
                        {
                            this.ids = this.ids + item.Value + ",";
                        }
                    }
                }
                if (ids.Count()>0&&ids[ids.Count()-1] == ',')
                {
                    ids.Remove(ids.Count() - 1);
                }
                await LoadData();
            }
        }

        private void TopBtnClicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            flipview.SelectedIndex = btn.TabIndex;
        }

        private void flipview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var view = sender as FlipView;
            switch (view.SelectedIndex)
            {
                case 0:
                    TuiJian_Btn.BorderThickness = new Thickness(0,0,0,2);
                    NewMV_Btn.BorderThickness = new Thickness(0);
                    MVLib_Btn.BorderThickness = new Thickness(0);
                    break;
                case 1:
                    TuiJian_Btn.BorderThickness = new Thickness(0);
                    NewMV_Btn.BorderThickness = new Thickness(0, 0, 0, 2);
                    MVLib_Btn.BorderThickness = new Thickness(0);
                    break;
                case 2:
                    TuiJian_Btn.BorderThickness = new Thickness(0);
                    NewMV_Btn.BorderThickness = new Thickness(0);
                    MVLib_Btn.BorderThickness = new Thickness(0, 0, 0, 2);
                    break;
                default:
                    break;
            }
        }

        private async void NewMVLoadMore(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv_SP = sender as ScrollViewer;
            if (sv_SP.VerticalOffset == sv_SP.ScrollableHeight&&newmvobj!=null&&LoadProcess.IsActive==false)
            {
                LoadProcess.IsActive = true;
                await newmvobj.LoadData();
                LoadProcess.IsActive = false;
            }
        }

        private async void LibSortBtnCLicked(object sender, RoutedEventArgs e)
        {
            LoadProcess.IsActive = true;
            var Theme = (Application.Current.Resources.ThemeDictionaries.ToList())[0].Value as ResourceDictionary;
            foreach (var menu in LibSortMenu.Items)
            {
                ((MenuFlyoutItem)menu).Foreground = ((SolidColorBrush)Theme["KuGou-Front1"]);
            }
            var sortmenu = sender as MenuFlyoutItem;
            sortmenu.Foreground = ((SolidColorBrush)Theme["KuGou-BackgroundColor"]);
            if (libmvdata != null)
            {
                await libmvdata.ChangeType((MVLibData.Type)sortmenu.TabIndex);
            }
            LoadProcess.IsActive = false;
        }

        private async void MVLibLoadMore(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv_SP = sender as ScrollViewer;
            if (sv_SP.VerticalOffset == sv_SP.ScrollableHeight && libmvdata != null && LoadProcess.IsActive == false)
            {
                LoadProcess.IsActive = true;
                await libmvdata.LoadData();
                LoadProcess.IsActive = false;
            }
        }

        private void mvlistView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            if (list.SelectedItem != null)
            {
                var data = list.SelectedItem as MVLibData.MVData;
                Frame.Navigate(typeof(Pages.MVPlayer), data.hash);
                list.SelectedIndex = -1;
            }
        }

        private async void NewMVTypeBtnCLicked(object sender, RoutedEventArgs e)
        {
            LoadProcess.IsActive = true;
            var Theme = (Application.Current.Resources.ThemeDictionaries.ToList())[0].Value as ResourceDictionary;
            foreach (var menu in NewMVTypeMenu.Items)
            {
                ((MenuFlyoutItem)menu).Foreground = ((SolidColorBrush)Theme["KuGou-Front1"]);
            }
            var typemenu = sender as MenuFlyoutItem;
            typemenu.Foreground = ((SolidColorBrush)Theme["KuGou-BackgroundColor"]);
            if (newmvobj != null)
            {
                await newmvobj.ChangeType((NewMVData.Type)typemenu.TabIndex);
            }
            LoadProcess.IsActive = false;
        }
    }
}
