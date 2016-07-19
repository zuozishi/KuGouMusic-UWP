using System;
using System.Collections.Generic;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace 酷狗音乐UWP.page
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WebPage : Page
    {
        public WebPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var url = e.Parameter.ToString();
            webview.NavigationCompleted += Webview_NavigationCompleted; ;
            webview.Source = new Uri(url);
        }

        private void Webview_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            Title_Text.Text = sender.DocumentTitle;
        }

        private void Back_Btn_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
