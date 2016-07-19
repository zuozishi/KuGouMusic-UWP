using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace 酷狗音乐UWP.UserControlClass
{
    public sealed partial class KanPage : UserControl
    {
        private DispatcherTimer BannerTimer = new DispatcherTimer();
        public KanPage()
        {
            this.InitializeComponent();
        }
        
        public void LoadData()
        {
            LoadBanner();
        }

        private async void LoadBanner()
        {
            BannerTimer.Interval = TimeSpan.FromSeconds(5);
            BannerTimer.Tick += (s, e) =>
            {
                if (BannerView.SelectedIndex == BannerView.Items.Count - 1)
                {
                    BannerView.SelectedIndex = 0;
                }
                else
                {
                    BannerView.SelectedIndex = BannerView.SelectedIndex + 1;
                }
            };
            if(BannerView.Items.Count==0)
            {
                var BannerData = await GetBanner();
                //<Image Source="http://imge.kugou.com/mobilebanner/20160708/20160708172910957734.jpg" Stretch="Fill"></Image>
                //<Ellipse Margin="0,0,5,0" Width="10" Height="10" Fill="#7FFFFFFF"></Ellipse>
                BannerTitle.Text = BannerData[0].title;
                for (int i = 0; i < BannerData.Count; i++)
                {
                    BannerView.Items.Add(BannerData[i]);
                    var ellipse = new Ellipse();
                    ellipse.Margin = new Thickness() { Right = 5 };
                    ellipse.Width = 10;
                    ellipse.Height = 10;
                    if (i == 0)
                    {
                        ellipse.Fill = new SolidColorBrush(Colors.White);
                    }
                    else
                    {
                        ellipse.Fill = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
                    }
                    BannerPanel.Children.Add(ellipse);
                }
                BannerTimer.Start();
            }
        }

        private async Task<List<BannerModel>> GetBanner()
        {
            var httpclient = new Noear.UWP.Http.AsyncHttpClient();
            httpclient.Url("http://service.mobile.kugou.com/v1/show/banner?type=1&plat=0&version=8150");
            var json = (await httpclient.Get()).GetString();
            var obj = Windows.Data.Json.JsonObject.Parse(json);
            var bannerdata = new List<BannerModel>();
            var banners = obj.GetNamedObject("data").GetNamedArray("info");
            foreach (var item in banners)
            {
                var banner = Class.data.DataContractJsonDeSerialize<BannerModel>(item.ToString());
                bannerdata.Add(banner);
            }
            return bannerdata;
        }

        public class BannerModel
        {
            public int type { get; set; }
            public string title { get; set; }
            public string imgurl { get; set; }
            public ExtraModel extra { get; set; }
        }

        public class ExtraModel
        {
            public int vid { get; set; }
            public string mvhash { get; set; }
            public string url { get; set; }
        }

        private void BannerView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var thispage = ((FlipView)sender).SelectedIndex;
                var item= (BannerModel)((FlipView)sender).SelectedItem;
                BannerTitle.Text = item.title;
                var ellipes = new List<Ellipse>();
                foreach (var items in BannerPanel.Children)
                {
                    ((Ellipse)items).Fill = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255));
                    ellipes.Add((Ellipse)items);
                }
                ellipes[thispage].Fill = new SolidColorBrush(Colors.White);
            }
            catch (Exception)
            {
                
            }
        }

        private void BannerView_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var view = sender as FlipView;
            if(view.SelectedItem!=null)
            {
                var data = view.SelectedItem as BannerModel;
                var frame = Window.Current.Content as Frame;
                switch (data.type)
                {
                    case 1:
                        frame.Navigate(typeof(page.MVPlayer), data.extra.mvhash);
                        break;
                    case 7:
                        frame.Navigate(typeof(page.WebPage), data.extra.url);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
