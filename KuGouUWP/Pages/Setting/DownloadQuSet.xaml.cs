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

namespace KuGouUWP.Pages.Setting
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DownloadQuSet : Page
    {
        public DownloadQuSet()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SIMQuList.SelectedIndex = (int)Class.Setting.DownQu.SIM;
            WiFiQuList.SelectedIndex = (int)Class.Setting.DownQu.WLAN;
        }

        private void ListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listview = sender as ListView;
            if (listview.Name == "SIMQuList")
            {
                Class.Setting.DownQu.SIM = (Class.Setting.DownQu.Type)listview.SelectedIndex;
            }
            else
            {
                Class.Setting.DownQu.WLAN = (Class.Setting.DownQu.Type)listview.SelectedIndex;
            }
        }

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
