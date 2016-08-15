using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace KuGouUWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
            init();
        }

        private async void init()
        {
            HttpClient hc = new HttpClient();
            HttpResponseMessage hr = await hc.GetAsync(new Uri("http://m.kugou.com/loginReg.php?act=login&url=http://kugou.com"));
            webview.Source = new Uri("http://m.kugou.com/loginReg.php?act=login&url=http://m.kugou.com");
            webview.NavigationCompleted += Webview_NavigationCompleted;
        }

        private void GetUserData(string cookie)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["cookie"] = cookie;
            var cookies = cookie.Split('&');
            foreach (var str in cookies)
            {
                if(str.Contains("KuGoo=KugooID="))
                {
                    var uids= str.Split('=');
                    localSettings.Values["uid"] = uids[uids.Count()-1];
                }
                if (str.Contains("Pic="))
                {
                    localSettings.Values["userpic"] = System.Net.WebUtility.UrlDecode(str.Replace("Pic=", ""));
                }
                if (str.Contains("NickName="))
                {
                    localSettings.Values["username"] = System.Net.WebUtility.UrlDecode(str.Replace("NickName=", ""));
                }
            }
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                Frame.Navigate(typeof(Pages.MyMusicListPage));
            }
        }

        private async void Webview_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            string js = "document.getElementsByClassName('header')[0].style.display='none';";
            if (args.Uri.ToString() == "http://m.kugou.com/loginReg.php?act=login&url=http://m.kugou.com")
            {
                title.Text = "登录";
            }
            if (args.Uri.ToString() == "http://m.kugou.com/loginReg.php?act=reg&url=http://m.kugou.com")
            {
                title.Text = "注册";
            }
            if (args.Uri.ToString()== "http://m.kugou.com/")
            {
                js = "window.document.title=document.cookie;";
            }
            try
            {
                await sender.InvokeScriptAsync("eval", new string[] { js });
                if (sender.DocumentTitle.Contains("KugooID"))
                {
                    GetUserData(sender.DocumentTitle);
                }
            }
            catch (Exception)
            {
                
            }
        }

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                Frame.Navigate(typeof(MainPage));
            }
        }
    }
}
