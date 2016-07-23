using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace 酷狗音乐UWP.page.Setting
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ListenQuSet : Page
    {
        public ListenQuSet()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SIMQuList.SelectedIndex = (int)Class.Setting.Qu.SIM;
            WiFiQuList.SelectedIndex=(int)Class.Setting.Qu.WLAN;
        }

        private void ListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listview = sender as ListView;
            if(listview.Name== "SIMQuList")
            {
                Class.Setting.Qu.SIM = (Class.Setting.Qu.Type)listview.SelectedIndex;
            }
            else
            {
                Class.Setting.Qu.WLAN = (Class.Setting.Qu.Type)listview.SelectedIndex;
            }
        }

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
