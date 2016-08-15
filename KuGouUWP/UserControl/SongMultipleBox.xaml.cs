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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace KuGouUWP.UserControlClass
{
    public sealed partial class SongMultipleBox : UserControl
    {
        public SongMultipleBox()
        {
            this.InitializeComponent();
        }
        
        public enum BtnType
        {
            NextPlay,Download,AddToList,SelectAll
        }

        public void Show()
        {
            mainGrid.Visibility = Visibility.Visible;
        }

        public void Hidden()
        {
            mainGrid.Visibility = Visibility.Collapsed;
        }

        public delegate void BtnClickedHandler(BtnType type);
        public event BtnClickedHandler BtnClickedEvent;

        private void BoxBtnClicked(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            BtnClickedEvent((BtnType)btn.TabIndex);
        }
    }
}
