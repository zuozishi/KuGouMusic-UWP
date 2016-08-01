using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KuGouMusicUWP.Class
{
    public class UserManager
    {
        static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public static bool isLogin()
        {
            if (localSettings.Values.ContainsKey("uid")&&localSettings.Values["uid"].ToString() != "")
            {
                return true;
            }else
            {
                return false;
            }
        }
        public static void ShowLoginUI()
        {
            var mainFrame = Window.Current.Content as Frame;
            mainFrame.Navigate(typeof(Pages.LoginPage));
        }
        public static void unLogin()
        {
            if (isLogin())
            {
                localSettings.Values.Remove("uid");
                localSettings.Values.Remove("userpic");
                localSettings.Values.Remove("username");
            }
        }

        public enum Type
        {
            uid,pic,name
        }
        public static string GetData(Type type)
        {
            if (isLogin())
            {
                var data = "";
                switch (type)
                {
                    case Type.uid:
                        data= (string)localSettings.Values["uid"];
                        break;
                    case Type.pic:
                        data = (string)localSettings.Values["userpic"];
                        if (!data.Contains("http://"))
                        {
                            string data1 = "http://imge.kugou.com/kugouicon/165/";
                            for (int i = 0; i < 8; i++)
                            {
                                data1 = data1 + data[i].ToString();
                            }
                            data = data1 +"/" +data;
                        }
                        break;
                    case Type.name:
                        data = (string)localSettings.Values["username"];
                        break;
                    default:
                        break;
                }
                return data;
            }
            else
            {
                return null;
            }
        }
    }
}
