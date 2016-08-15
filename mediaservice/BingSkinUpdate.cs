using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;

namespace mediaservice
{
    public sealed class BingSkinUpdate : IBackgroundTask
    {
        static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        static StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            if (localSettings.Values.ContainsKey("Theme")&& (int)localSettings.Values["Theme"]==4)
            {
                try
                {
                    var httpclient = new System.Net.Http.HttpClient();
                    var picbytes = await httpclient.GetByteArrayAsync("http://appserver.m.bing.net/BackgroundImageService/TodayImageService.svc/GetTodayImage?dateOffset=0&urlEncodeHeaders=true&osName=windowsPhone&osVersion=8.10&orientation=480x800&deviceName=WP8&mkt=en-US");
                    var picfile = await localFolder.CreateFileAsync("bingbackground.jpg", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteBytesAsync(picfile, picbytes);
                    localSettings.Values["isReloadBingSkin"] = true;
                    Debug.WriteLine("BingSkinUpdateSuccess");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
            deferral.Complete();
        }
        
        private class BingThemeData
        {
            public Color BackgroundColor { get; set; }
            public Color BackgroundColor1 { get; set; }
            public Color BackgroundColor2 { get; set; }
            public Color Foreground { get; set; }
            public Color Front1 { get; set; }
            public Color Front2 { get; set; }
            public Color List_Background { get; set; }
        }
    }
}
