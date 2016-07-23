using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace 酷狗音乐UWP.Class
{
    public class Setting
    {
        public static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public class Qu
        {
            public enum Type
            {
                low,mid,high
            }
            public static Type SIM
            {
                get
                {
                    if(!localSettings.Values.ContainsKey("SIMQu"))
                    {
                        localSettings.Values["SIMQu"] = 0;
                        return Type.low;
                    }
                    else
                    {
                        return (Type)(int)localSettings.Values["SIMQu"];
                    }
                }
                set
                {
                    localSettings.Values["SIMQu"] = (int)value;
                }
            }
            public static Type WLAN
            {
                get
                {
                    if (!localSettings.Values.ContainsKey("WLANQu"))
                    {
                        localSettings.Values["WLANQu"] = 0;
                        return Type.low;
                    }
                    else
                    {
                        return (Type)(int)localSettings.Values["WLANQu"];
                    }
                }
                set
                {
                    localSettings.Values["WLANQu"] = (int)value;
                }
            }
            public static new Type GetType()
            {
                ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
                if (InternetConnectionProfile.IsWlanConnectionProfile)
                {
                    return WLAN;
                }
                else
                {
                    return SIM;
                }
            }
        }
        public class Theme
        {
            public enum Type
            {
                Default,BiShuiLan,StarNight,Rabbit
            }
            public class ThemeList
            {
                public class ThemeData
                {
                    public string title { get; set; }
                    public string img { get; set; }
                }
                public List<ThemeData> List { get; set; }
            }
            public static Type NowTheme
            {
                get
                {
                    if (!localSettings.Values.ContainsKey("Theme"))
                    {
                        return Type.Default;
                    }
                    else
                    {
                        return (Type)(int)localSettings.Values["Theme"];
                    }
                }
                set
                {
                    localSettings.Values["Theme"] = (int)value;
                }
            }
        }
    }
}
