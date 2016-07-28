using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI;
using System.Threading.Tasks;
using Windows.Storage;
using System.Collections.ObjectModel;
using Windows.Media.Core;
using static 酷狗音乐UWP.Class.Model.LocalList;
using Windows.ApplicationModel.Background;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Shapes;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace 酷狗音乐UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer SongProgressTimer = new DispatcherTimer();
        public MediaPlayer BackGroundPlayer { get; private set; }

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            init();
            注册后台服务();
        }

        private void 注册后台服务()
        {
            try
            {
                foreach (var cur in BackgroundTaskRegistration.AllTasks)
                {
                    if (cur.Value.Name == "TitleTask")
                    {
                        return;
                    }
                }
                var builder = new BackgroundTaskBuilder();
                builder.Name = "TitleTask";
                builder.TaskEntryPoint = "mediaservice.TitleUpdateTask";
                builder.SetTrigger(new TimeTrigger(15, false));
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                var task = builder.Register();
            }
            catch (Exception)
            {

            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (Class.UserManager.isLogin())
            {
                var ellipse = new Ellipse();
                ellipse.Width = 45;
                ellipse.Height = 45;
                ellipse.Margin = new Thickness(-10);
                var imgsource = new Windows.UI.Xaml.Media.Imaging.BitmapImage() { UriSource = new Uri(Class.UserManager.GetData(Class.UserManager.Type.pic)) };
                ellipse.Fill = new ImageBrush() { ImageSource = imgsource };
                UserLoginBtn.Content = ellipse;
            }
            else
            {
                UserLoginBtn.Content = new SymbolIcon(Symbol.Contact2);
            }
        }

        private async void init()
        {
            LoadProgress.IsActive = true;
            var appfolder = ApplicationData.Current.LocalFolder;
            var localSettings = ApplicationData.Current.LocalSettings;
            if(!localSettings.Values.ContainsKey("isfirst"))
            {
                localSettings.Values["StartVoice"] = false;
                localSettings.Values["StartVoiceId"] = 0;
                var datafolder=await appfolder.CreateFolderAsync("Data",CreationCollisionOption.ReplaceExisting);
                await appfolder.CreateFolderAsync("Temp", CreationCollisionOption.ReplaceExisting);
                await datafolder.CreateFileAsync("localfolders.json", CreationCollisionOption.ReplaceExisting);
                await datafolder.CreateFileAsync("locallist.json", CreationCollisionOption.ReplaceExisting);
                await datafolder.CreateFileAsync("nowplay.json", CreationCollisionOption.ReplaceExisting);
                await datafolder.CreateFileAsync("nowplay.lrc", CreationCollisionOption.ReplaceExisting);
                await datafolder.CreateFileAsync("playlist.json", CreationCollisionOption.ReplaceExisting);
                localSettings.Values["isfirst"] = true;
            }
            var Theme = (Application.Current.Resources.ThemeDictionaries.ToList())[1].Value as ResourceDictionary;
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = Colors.White;
                statusBar.BackgroundColor = ((SolidColorBrush)Theme["KuGou-BackgroundColor"]).Color;
                statusBar.BackgroundOpacity = 100;
            }
            //电脑标题栏颜色
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = ((SolidColorBrush)Theme["KuGou-BackgroundColor"]).Color;
            titleBar.ButtonBackgroundColor = ((SolidColorBrush)Theme["KuGou-BackgroundColor"]).Color;
            titleBar.ForegroundColor = Color.FromArgb(255, 254, 254, 254);//Colors.White纯白用不了
            if (Class.UserManager.isLogin())
            {
                var ellipse = new Ellipse();
                ellipse.Width = 45;
                ellipse.Height = 45;
                ellipse.Margin = new Thickness(-10);
                var imgsource= new Windows.UI.Xaml.Media.Imaging.BitmapImage() { UriSource = new Uri(Class.UserManager.GetData(Class.UserManager.Type.pic)) };
                ellipse.Fill= new ImageBrush() { ImageSource = imgsource};
                UserLoginBtn.Content = ellipse;
            }else
            {
                UserLoginBtn.Content = new SymbolIcon(Symbol.Contact2);
            }
            //await Class.Model.PlayList.Clear();
            KanPagePanel.LoadData();
            await init_local_list();
            CheckFeedBack();
            LoadProgress.IsActive = false;
        }

        private async void CheckFeedBack()
        {
            try
            {
                var userInfo = await JyUserInfo.JyUserInfoManager.QuickLogin("e4e6005e3145b90b4edd99c0d0d35af9");
                if (userInfo.isLoginSuccess)
                {
                    var feedback = new JyUserFeedback.JyUserFeedbackSDKManager();
                    var msgnum = await feedback.GetNewFeedBackRemindCount("e4e6005e3145b90b4edd99c0d0d35af9", userInfo.U_Key);
                    if (msgnum > 0)
                    {
                        await new Windows.UI.Popups.MessageDialog("您收到" + msgnum + "条反馈回复,请到设置->反馈查看").ShowAsync();
                    }
                }
            }
            catch (Exception)
            {
                
            }
        }

        private async Task init_local_list()
        {
            var Items = await Class.Model.LocalList.GetList();
            if (Items != null)
            {
                this.itemcollectSource.Source = Items;
                ZoomOutView.ItemsSource = itemcollectSource.View.CollectionGroups;
                ZoomInView.ItemsSource = itemcollectSource.View;
            }
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(page.Search));
        }

        private void flipview1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var flipview = sender as FlipView;
            var Theme = (Application.Current.Resources.ThemeDictionaries.ToList())[0].Value as ResourceDictionary;
            switch (flipview.SelectedIndex)
            {
                case 0:
                    
                    Ting_Btn.BorderBrush = (SolidColorBrush)Theme["KuGou-Foreground"];
                    Kan_Btn.BorderBrush = null;
                    Chang_Btn.BorderBrush = null;
                    break;
                case 1:
                    Ting_Btn.BorderBrush = null;
                    Kan_Btn.BorderBrush = (SolidColorBrush)Theme["KuGou-Foreground"];
                    Chang_Btn.BorderBrush = null;
                    break;
                case 2:
                    Ting_Btn.BorderBrush = null;
                    Kan_Btn.BorderBrush = null;
                    Chang_Btn.BorderBrush = (SolidColorBrush)Theme["KuGou-Foreground"];
                    break;
                default:
                    break;
            }
        }

        private void Play_Btn_Click(object sender, RoutedEventArgs e)
        {
            if(BackGroundPlayer.CurrentState==MediaPlayerState.Playing)
            {
                BackGroundPlayer.Pause();
            }
            else
            {
                BackGroundPlayer.Play();
            }
        }

        private async void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            try
            {
                if (sender.Text.Length != 0)
                {
                    //ObservableCollection<String>
                    var data = new ObservableCollection<string>();
                    var httpclient = new Noear.UWP.Http.AsyncHttpClient();
                    httpclient.Url("http://mobilecdn.kugou.com/new/app/i/search.php?cmd=302&keyword=" + sender.Text);
                    var httpresult = await httpclient.Get();
                    var jsondata = httpresult.GetString();
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

        private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (sender.Text.Length > 0)
            {
                await Task.Delay(100);
                Frame.Navigate(typeof(page.SearchResult), sender.Text);
            }
        }

        private async void UserLoginBtn_Click(object sender, RoutedEventArgs e)
        {
            //Frame.Navigate(typeof(page.WebPage), "http://m.kugou.com/song/static/index.html");
            if (Class.UserManager.isLogin())
            {
                var menu= new MessageDialog("是否注销当前用户?");
                UICommand cmdOK = new UICommand("确定",new UICommandInvokedHandler(UnloginOnCommandAct),1);
                UICommand cmdCancel = new UICommand("取消", new UICommandInvokedHandler(UnloginOnCommandAct), 2);
                menu.Commands.Add(cmdOK);
                menu.Commands.Add(cmdCancel);
                await menu.ShowAsync();
            }
            else
            {
                Frame.Navigate(typeof(page.LoginPage));
            }
        }

        private void UnloginOnCommandAct(IUICommand command)
        {
            if((int)(command.Id)==1)
            {
                Class.UserManager.unLogin();
            }
            if (Class.UserManager.isLogin())
            {
                var ellipse = new Ellipse();
                ellipse.Width = 45;
                ellipse.Height = 45;
                ellipse.Margin = new Thickness(-10);
                var imgsource = new Windows.UI.Xaml.Media.Imaging.BitmapImage() { UriSource = new Uri(Class.UserManager.GetData(Class.UserManager.Type.pic)) };
                ellipse.Fill = new ImageBrush() { ImageSource = imgsource };
                UserLoginBtn.Content = ellipse;
            }
            else
            {
                UserLoginBtn.Content = new SymbolIcon(Symbol.Contact2);
            }
        }

        private void TopBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            flipview1.SelectedIndex = btn.TabIndex;
        }

        private async void LocalList_Selecyion(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                LoadProgress.IsActive = true;
                var list = sender as ListView;
                if(list.SelectedIndex!=-1)
                {
                    if (list.SelectedItem != null)
                    {
                        var data = list.SelectedItem as Music;
                        var nowplay = new Class.Model.Player.NowPlay();
                        if (data.path == null && data.path == "")
                        {
                            LoadProgress.IsActive = false;
                            return;
                        }
                        nowplay.url = data.path;
                        nowplay.title = data.Title;
                        nowplay.singername = data.songer;
                        nowplay.albumid = "";
                        nowplay.imgurl = "ms-appx:///Assets/image/songimg.png";
                        await Class.Model.PlayList.Add(nowplay, true);
                    }
                    list.SelectedIndex = -1;
                }
                LoadProgress.IsActive = false;
            }
            catch (Exception)
            {
                
            }
        }

        private async void LocalListRefreshBtn_Clicked(object sender, RoutedEventArgs e)
        {
            LoadProgress.IsActive = true;
            await Class.Model.LocalList.UpdateList();
            var Items = await Class.Model.LocalList.GetList();
            this.itemcollectSource.Source = Items;
            ZoomOutView.ItemsSource = itemcollectSource.View.CollectionGroups;
            ZoomInView.ItemsSource = itemcollectSource.View;
            LoadProgress.IsActive = false;
        }

        private async void AddLocalFolderBtn_Clicked(object sender, RoutedEventArgs e)
        {
            LoadProgress.IsActive = true;
            await Class.Model.LocalList.AddFolder();
            await Class.Model.LocalList.UpdateList();
            var Items = await Class.Model.LocalList.GetList();
            this.itemcollectSource.Source = Items;
            ZoomOutView.ItemsSource = itemcollectSource.View.CollectionGroups;
            ZoomInView.ItemsSource = itemcollectSource.View;
            LoadProgress.IsActive = false;
        }

        private void YueKuBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(page.YueKuPage));
        }

        private void DownloadBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(page.DownloadPage));
        }

        private void CloudMuiscBtn_Clicked(object sender, RoutedEventArgs e)
        {
            if (Class.UserManager.isLogin())
            {
                Frame.Navigate(typeof(page.MyMusicListPage));
            }
            else
            {
                Class.UserManager.ShowLoginUI();
            }
        }

        private void SettingBtnClicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(page.SettingPage));
        }

        private void HistoryBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(page.HistoryPage));
        }
    }
}
