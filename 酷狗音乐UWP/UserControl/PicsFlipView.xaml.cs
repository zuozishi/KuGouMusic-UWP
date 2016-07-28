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
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace 酷狗音乐UWP.UserControlClass
{
    public sealed partial class PicsFlipView : UserControl
    {
        private List<string> titledata;
        private DispatcherTimer timer = new DispatcherTimer();
        private int thispage = 0;
        public FlipView picflipview;

        public PicsFlipView()
        {
            this.InitializeComponent();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Timer_Tick;
            picflipview = flipview;
        }

        private void Timer_Tick(object sender, object e)
        {
            if(flipview.Items.Count>0)
            {
                if(thispage==flipview.Items.Count-1)
                {
                    thispage = 0;
                }
                else
                {
                    thispage = thispage + 1;
                }
                flipview.SelectedIndex = thispage;
            }
        }

        public void SetItems(List<string> imgurls)
        {
            titledata = null;
            flipview.Items.Clear();
            foot_Panel.Children.Clear();
            foot_Panel.HorizontalAlignment = HorizontalAlignment.Center;
            foreach (var item in imgurls)
            {
                var image = new Image();
                image.Stretch = Stretch.Fill;
                image.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage() { UriSource = new Uri(item) };
                flipview.Items.Add(image);
                var ellipse = new Ellipse();
                ellipse.Style = (Style)Resources["unnow"];
                foot_Panel.Children.Add(ellipse);
            }
            flipview.SelectedIndex = 0;
            timer.Start();
        }

        public void ClearItems()
        {
            titledata = null;
            flipview.Items.Clear();
            foot_Panel.Children.Clear();
        }

        public void SetItems(List<string> imgurls, List<string> titles)
        {
            titledata = null;
            flipview.Items.Clear();
            foot_Panel.Children.Clear();
            foot_Panel.HorizontalAlignment = HorizontalAlignment.Right;
            titledata = titles;
            foreach (var item in imgurls)
            {
                var image = new Image();
                image.Stretch = Stretch.Fill;
                image.Source = new Windows.UI.Xaml.Media.Imaging.BitmapImage() { UriSource = new Uri(item) };
                flipview.Items.Add(image);
            }
            flipview.SelectedIndex = 0;
            timer.Start();
        }

        private void flipview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var view = sender as FlipView;
            var num = view.SelectedIndex;
            if (titledata != null && titledata.Count > 0)
            {
                title_Text.Text = titledata[num];
            }
            for (int i = 0; i < foot_Panel.Children.Count; i++)
            {
                var ellpise = foot_Panel.Children[i] as Ellipse;
                if(num==i)
                {
                    ellpise.Style = (Style)Resources["isnow"];
                }
                else
                {
                    ellpise.Style = (Style)Resources["unnow"];
                }
            }
        }
    }
}
