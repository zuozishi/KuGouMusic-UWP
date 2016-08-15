using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace KuGouUWP
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        StorageFolder localFolder = ApplicationData.Current.LocalFolder;
        StorageFolder appFolder = ApplicationData.Current.LocalFolder;
        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = false;
            }
#endif
            Frame rootFrame = Window.Current.Content as Frame;
            await InitBingSkin();
            KuGouUWP.Class.Setting.Theme.NowTheme = KuGouUWP.Class.Setting.Theme.NowTheme;
            Windows.Media.Playback.BackgroundMediaPlayer.IsMediaPlaying();
            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    rootFrame.CacheSize = 5;
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                SystemNavigationManager.GetForCurrentView().BackRequested += App_BackRequested;
                await PlayWelcomeVoice();
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);
            if (args.Kind == ActivationKind.VoiceCommand)
            {
                var vcargs = (VoiceCommandActivatedEventArgs)args;
                var res = vcargs.Result;
                var cmdName = res.RulePath[0];
                var root = Window.Current.Content as Frame;
                if (root == null)
                {
                    root = new Frame();
                    Window.Current.Content = root;
                }
                switch (cmdName)
                {
                    case "Search":
                        var content = res.Text.Remove(0, 2);
                        content=content.Replace("。", "");
                        content=content.Replace(".", "");
                        root.Navigate(typeof(Pages.SearchResult), content);
                        break;
                    case "OpenMyList":
                        if(Class.UserManager.isLogin())
                        {
                            root.Navigate(typeof(Pages.MyMusicListPage));
                        }
                        else
                        {
                            root.Navigate(typeof(Pages.LoginPage));
                        }
                        break;
                    case "OpenYueKu":root.Navigate(typeof(Pages.YueKuPage));break;
                    case "OpenSetting": root.Navigate(typeof(Pages.SettingPage)); break;
                    case "OpenPlaying": root.Navigate(typeof(Pages.PlayerPgae)); break;
                }
                Window.Current.Activate();
            }
        }

        private async Task InitBingSkin()
        {
            if(localSettings.Values.ContainsKey("isReloadBingSkin") &&(bool)localSettings.Values["isReloadBingSkin"])
            {
                var tempfile = await localFolder.CreateFileAsync("bingbackground.jpg", CreationCollisionOption.OpenIfExists);
                var picfile= await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Theme/Skin/Bing/background.jpg"));
                await FileIO.WriteBufferAsync(picfile,await FileIO.ReadBufferAsync(tempfile));
                WriteableBitmap wb = new WriteableBitmap(1000, 1500);
                await wb.SetSourceAsync(await picfile.OpenAsync(FileAccessMode.Read));
                var bingtheme = new Pages.Setting.SkinSetPage.BingThemeData();
                bingtheme.BackgroundColor = ImageUtility.GetColor.GetMajorColor(wb);
                Debug.WriteLine("MainColor:" + bingtheme.BackgroundColor.ToString());
                bingtheme.BackgroundColor1 = Color.FromArgb(190, bingtheme.BackgroundColor.R, bingtheme.BackgroundColor.G, bingtheme.BackgroundColor.B);
                var rgb = bingtheme.BackgroundColor.R * 0.299 + bingtheme.BackgroundColor.G * 0.587 + bingtheme.BackgroundColor.B * 0.114;
                bingtheme.BackgroundColor2 = bingtheme.BackgroundColor;
                if (rgb >= 192)
                {
                    bingtheme.Foreground = Colors.White;
                    bingtheme.Front1 = Colors.White;
                    bingtheme.Front2 = Color.FromArgb(255, 185, 185, 185);
                    bingtheme.List_Background = Color.FromArgb(100, 103, 103, 103);
                }
                else
                {
                    bingtheme.Foreground = Colors.White;
                    bingtheme.Front1 = Colors.Black;
                    bingtheme.Front2 = Color.FromArgb(255, 124, 124, 124);
                    bingtheme.List_Background = Color.FromArgb(180, 255, 255, 255);
                }
                string xamlstring = await PathIO.ReadTextAsync("ms-appx:///Theme/BingTemp.txt");
                xamlstring = String.Format(xamlstring, bingtheme.BackgroundColor.ToString(), bingtheme.BackgroundColor1.ToString(), bingtheme.BackgroundColor2.ToString(), bingtheme.Foreground.ToString(), bingtheme.Front1.ToString(), bingtheme.Front2.ToString(), bingtheme.List_Background.ToString());
                var xamlfile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Theme/BingTheme.xaml"));
                await FileIO.WriteTextAsync(xamlfile, xamlstring);
                localSettings.Values["isReloadBingSkin"] = false;
                Debug.WriteLine("BingSkinLoadSuccess");
            }
        }

        private async Task PlayWelcomeVoice()
        {
            var isopen = (bool)localSettings.Values["StartVoice"];
            await JYAnalyticsUniversal.JYAnalytics.StartTrackAsync("e4e6005e3145b90b4edd99c0d0d35af9");
            if (isopen)
            {
                var media = new MediaElement();
                media.AutoPlay = true;
                media.AudioCategory = AudioCategory.Alerts;
                var id = (int)localSettings.Values["StartVoiceId"];
                if (id == 0)
                {
                    var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/welcome.mp3"));
                    media.SetSource(await file.OpenReadAsync(), file.ContentType);
                }
                else
                {
                    var file = await (await appFolder.CreateFolderAsync("WelcomeVoice", CreationCollisionOption.OpenIfExists)).GetFileAsync(id.ToString() + ".mp3");
                    media.SetSource(await file.OpenReadAsync(), file.ContentType);
                }
                await Task.Delay(3000);
            }
        }

        private void App_BackRequested(object sender, BackRequestedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
                return;

            // Navigate back if possible, and if the event has not 
            // already been handled .
            if (rootFrame.CanGoBack && e.Handled == false)
            {
                e.Handled = true;
                rootFrame.GoBack();
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            await JYAnalyticsUniversal.JYAnalytics.EndTrackAsync();
            deferral.Complete();
        }
    }
}
