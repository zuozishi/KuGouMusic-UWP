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

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace KuGouUWP.Pages.Setting
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LocalFlodersSet : Page
    {
        private ObservableCollection<string> folders;

        public LocalFlodersSet()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            folders = await Class.Model.LocalList.GetFolder();
            if (folders != null)
            {
                listview.ItemsSource = folders;
            }
        }

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void AddFolderBtn_Clicked(object sender, RoutedEventArgs e)
        {
            await Class.Model.LocalList.AddFolder();
            folders = await Class.Model.LocalList.GetFolder();
            if (folders != null)
            {
                listview.ItemsSource = folders;
            }
        }

        private async void DelFolderBtn_Clicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            var data = btn.DataContext.ToString();
            for (int i = 0; i < folders.Count; i++)
            {
                if (data == folders[i])
                {
                    folders.RemoveAt(i);
                    await Class.Model.LocalList.DelFolder(i);
                }
            }
        }
    }
}
