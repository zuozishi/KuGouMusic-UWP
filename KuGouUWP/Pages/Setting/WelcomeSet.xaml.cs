using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace KuGouUWP.Pages.Setting
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WelcomeSet : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        StorageFolder appFolder = ApplicationData.Current.LocalFolder;
        private ObservableCollection<VoiceData> voicedata;

        public WelcomeSet()
        {
            this.InitializeComponent();
            init();
        }

        private void init()
        {
            voiceSwitch.IsOn = (bool)localSettings.Values["StartVoice"];
            if (voiceSwitch.IsOn)
            {
                listview.Visibility = Visibility.Visible;
            }
            else
            {
                listview.Visibility = Visibility.Collapsed;
            }
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                var httpclient = new System.Net.Http.HttpClient();
                var json = await httpclient.GetStringAsync("http://tools.mobile.kugou.com/api/v1/welcome/list?plat=0&version=8150");
                var obj = Windows.Data.Json.JsonObject.Parse(json);
                voicedata = Class.data.DataContractJsonDeSerialize<ObservableCollection<VoiceData>>(obj.GetNamedObject("data").GetNamedArray("info").ToString());
                voicedata.Insert(0, new VoiceData() { name = "经典版", id = 0, source = "ms-appx:///Assets/welcome.mp3" });
                var nowid = (int)localSettings.Values["StartVoiceId"];
                foreach (var item in voicedata)
                {
                    if (item.id == nowid)
                    {
                        item.isnow = "Visible";
                        break;
                    }
                }
                listview.ItemsSource = voicedata;
                listview.SelectionMode = ListViewSelectionMode.Single;
                listview.SelectionChanged += Listview_SelectionChanged;
            }
            catch (Exception e)
            {
                await new Windows.UI.Popups.MessageDialog("Error:"+ e.Message).ShowAsync();
            }
        }

        private void Listview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            var voice = listview.SelectedItem as VoiceData;
            Player(voice);
            foreach (var item in voicedata)
            {
                item.isnow = "Collapsed";
                item.RaisePropertyChanged("isnow");
            }
            voice.isnow = "Visible";
            voice.RaisePropertyChanged("isnow");
            localSettings.Values["StartVoiceId"]=voice.id;
        }

        private async void Player(VoiceData data)
        {
            if(data.source.Contains("http://"))
            {
                var file = await (await appFolder.CreateFolderAsync("WelcomeVoice", CreationCollisionOption.OpenIfExists)).CreateFileAsync(data.id.ToString() + ".mp3", CreationCollisionOption.ReplaceExisting);
                var downer = new Windows.Networking.BackgroundTransfer.BackgroundDownloader();
                var downop= downer.CreateDownload(new Uri(data.source), file);
                await downop.StartAsync();
                mediaplayer.SetSource(await file.OpenReadAsync(), file.ContentType);
            }
            else
            {
                var file =await StorageFile.GetFileFromApplicationUriAsync(new Uri(data.source));
                mediaplayer.SetSource(await file.OpenReadAsync(), file.ContentType);
            }
        }

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void voiceSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            var vswitch = sender as ToggleSwitch;
            localSettings.Values["StartVoice"]=vswitch.IsOn;
            if (vswitch.IsOn)
            {
                listview.Visibility = Visibility.Visible;
            }
            else
            {
                listview.Visibility = Visibility.Collapsed;
            }
        }

        public class VoiceData: INotifyPropertyChanged
        {
            public string name { get; set; }
            public int id { get; set; }
            public string source { get; set; }
            public string isnow { get; set; }
            public VoiceData()
            {
                isnow = "Collapsed";
            }

            public event PropertyChangedEventHandler PropertyChanged;
            public void RaisePropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
