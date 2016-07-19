using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace 酷狗音乐UWP.UserControlClass
{
    public sealed partial class PlayerSmall : UserControl
    {
        public MediaPlayer BackgroundMedia { get; private set; }
        private DispatcherTimer ProcessTimer=new DispatcherTimer();
        private Frame mainFrame;
        private Class.Model.Player.NowPlay nowplay;
        public string pageweight { get; set; }

        public PlayerSmall()
        {
            this.InitializeComponent();
            init();
        }

        private async void init()
        {
            mainFrame = Window.Current.Content as Frame;
            BackgroundMedia = Class.MediaControl.GetCurrent();
            BackgroundMedia.CurrentStateChanged += BackgroundMedia_CurrentStateChanged;
            ProcessTimer.Interval = TimeSpan.FromMilliseconds(1000);
            ProcessTimer.Tick += ProcessTimer_Tick;
            nowplay = await Class.Model.PlayList.GetNowPlay();
            if(nowplay!=null)
            {
                SongName_Text.Text = nowplay.title;
                SingerName_Text.Text = nowplay.singername;
                Singer_Image.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage() { UriSource = new Uri(nowplay.imgurl) };
            }
            else
            {
                SongName_Text.Text = "酷狗音乐";
                SingerName_Text.Text = "传播好音乐";
                Singer_Image.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage() { UriSource = new Uri("ms-appx:///Assets/image/songimg.png") };
            }
            if(BackgroundMedia.CurrentState==MediaPlayerState.Playing)
            {
                Play_Btn_Icon.Symbol = Symbol.Pause;
                ProcessTimer.Start();
            }
            else
            {
                Play_Btn_Icon.Symbol = Symbol.Play;
            }
            //ProcessTimer.Start();
        }

        private void BackgroundMedia_CurrentStateChanged(MediaPlayer sender, object args)
        {
            var state = sender.CurrentState;
            mainFrame.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
             {
                 switch (state)
                 {
                     case MediaPlayerState.Closed:
                         Play_Btn_Icon.Symbol = Symbol.Play;
                         ProcessTimer.Stop();
                         break;
                     case MediaPlayerState.Opening:
                         Play_Btn_Icon.Symbol = Symbol.Pause;
                         break;
                     case MediaPlayerState.Buffering:
                         Song_Progress.IsIndeterminate = true;
                         break;
                     case MediaPlayerState.Playing:
                         Play_Btn_Icon.Symbol = Symbol.Pause;
                         ProcessTimer.Start();
                         Song_Progress.IsIndeterminate = false;
                         nowplay = await Class.Model.Player.GetNowPlay();
                         SongName_Text.Text = nowplay.title;
                         SingerName_Text.Text = nowplay.singername;
                         Singer_Image.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage() { UriSource = new Uri(nowplay.imgurl) };
                         break;
                     case MediaPlayerState.Paused:
                         Play_Btn_Icon.Symbol = Symbol.Play;
                         ProcessTimer.Stop();
                         break;
                     case MediaPlayerState.Stopped:
                         Play_Btn_Icon.Symbol = Symbol.Play;
                         ProcessTimer.Stop();
                         break;
                     default:
                         break;
                 }
             });
        }

        private async void ProcessTimer_Tick(object sender, object e)
        {
            Song_Progress.Maximum = BackgroundMedia.NaturalDuration.TotalSeconds; ;
            Song_Progress.Value = BackgroundMedia.Position.TotalSeconds;
            if (nowplay == null)
            {
                nowplay = await Class.Model.Player.GetNowPlay();
                SongName_Text.Text = nowplay.title;
                SingerName_Text.Text = nowplay.singername;
                Singer_Image.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage() { UriSource = new Uri(nowplay.imgurl) };
            }
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            mainFrame.Navigate(typeof(page.PlayerPgae));
        }

        private void Play_Btn_Click(object sender, RoutedEventArgs e)
        {
            if(Song_Progress.Value==0)
            {
                Class.Model.PlayList.Play();
            }
            if(BackgroundMedia.CurrentState==MediaPlayerState.Playing)
            {
                BackgroundMedia.Pause();
            }
            else
            {
                BackgroundMedia.Play();
            }
        }

        private void Next_Btn_Click(object sender, RoutedEventArgs e)
        {
            Class.Model.PlayList.Next();
        }
    }
}
