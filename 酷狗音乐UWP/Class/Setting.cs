using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KuGouMusicUWP.Class
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
        public class DownQu
        {
            public enum Type
            {
                low, mid, high
            }
            public static Type SIM
            {
                get
                {
                    if (!localSettings.Values.ContainsKey("DownSIMQu"))
                    {
                        localSettings.Values["DownSIMQu"] = 0;
                        return Type.low;
                    }
                    else
                    {
                        return (Type)(int)localSettings.Values["DownSIMQu"];
                    }
                }
                set
                {
                    localSettings.Values["DownSIMQu"] = (int)value;
                }
            }
            public static Type WLAN
            {
                get
                {
                    if (!localSettings.Values.ContainsKey("DownWLANQu"))
                    {
                        localSettings.Values["DownWLANQu"] = 2;
                        return Type.high;
                    }
                    else
                    {
                        return (Type)(int)localSettings.Values["DownWLANQu"];
                    }
                }
                set
                {
                    localSettings.Values["DownWLANQu"] = (int)value;
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
                    ResourceDictionary newDictionary = new ResourceDictionary();
                    switch (value)
                    {
                        case Class.Setting.Theme.Type.Default:
                            newDictionary.Source = new Uri("ms-appx:///Theme/DefaultTheme.xaml");
                            Application.Current.Resources.MergedDictionaries.Clear();
                            Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                            break;
                        case Class.Setting.Theme.Type.BiShuiLan:
                            newDictionary.Source = new Uri("ms-appx:///Theme/BiShuiLanTheme.xaml");
                            Application.Current.Resources.MergedDictionaries.Clear();
                            Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                            break;
                        case Class.Setting.Theme.Type.StarNight:
                            newDictionary.Source = new Uri("ms-appx:///Theme/StarNightTheme.xaml");
                            Application.Current.Resources.MergedDictionaries.Clear();
                            Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                            break;
                        case Class.Setting.Theme.Type.Rabbit:
                            newDictionary.Source = new Uri("ms-appx:///Theme/RabbitTheme.xaml");
                            Application.Current.Resources.MergedDictionaries.Clear();
                            Application.Current.Resources.MergedDictionaries.Add(newDictionary);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
