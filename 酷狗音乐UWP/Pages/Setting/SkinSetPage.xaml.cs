using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace KuGouMusicUWP.Pages.Setting
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SkinSetPage : Page
    {
        public SkinSetPage()
        {
            this.InitializeComponent();
            init();
        }

        private void init()
        {
            ThemeList.ItemsSource = Theme.GetThemes();
            ThemeList.SelectionMode = ListViewSelectionMode.Single;
            ThemeList.SelectionChanged += ThemeList_SelectionChanged;
        }

        private void ThemeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as GridView;
            if (list.SelectedItem != null)
            {
                var data = list.SelectedItem as Theme;
                Class.Setting.Theme.NowTheme = data.theme;
                ShowDialog();
                list.SelectedIndex = -1;
                init();
            }
        }

        private async void ShowDialog()
        {
            var dialog = new MessageDialog(ResourceLoader.GetForCurrentView().GetString("ThemeLoadDialog"));
            dialog.Commands.Add(new UICommand(ResourceLoader.GetForCurrentView().GetString("RestartBtn"), DialogHandler,0));
            dialog.Commands.Add(new UICommand(ResourceLoader.GetForCurrentView().GetString("LaterBtn"), DialogHandler, 1));
            await dialog.ShowAsync();
        }

        private void DialogHandler(IUICommand command)
        {
            var num = (int)(command.Id);
            if (num == 0)
            {
                Application.Current.Exit();
            }
        }

        public class Theme
        {
            public int id { get; set; }
            public string title { get; set; }
            public string pic { get; set; }
            public string isnow
            {
                get
                {
                    if (theme == Class.Setting.Theme.NowTheme)
                    {
                        return "Visible";
                    }
                    else
                    {
                        return "Collapsed";
                    }
                }
            }
            public Class.Setting.Theme.Type theme
            {
                get
                {
                    return (Class.Setting.Theme.Type)id;
                }
            }
            public static ObservableCollection<Theme> GetThemes()
            {
                var result = new ObservableCollection<Theme>();
                result.Add(new Theme { id=0,title="默认皮肤",pic= "ms-appx:///Theme/Skin/Default/thumb.png" });
                result.Add(new Theme { id = 1, title = "碧水蓝", pic = "ms-appx:///Theme/Skin/BiShuiLan/thumb.png" });
                result.Add(new Theme { id = 2, title = "星空", pic = "ms-appx:///Theme/Skin/StarNight/thumb.png" });
                result.Add(new Theme { id = 3, title = "兔子的梦境", pic = "ms-appx:///Theme/Skin/Rabbit/thumb.png" });
                return result;
            }
        }

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
