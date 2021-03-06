﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using JyUserFeedback;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Resources;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上有介绍

namespace KuGouUWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        private JyUserFeedbackSDKManager feedback;
        public delegate void GoBackHandler();

        public SettingPage()
        {
            this.InitializeComponent();
        }

        private void BackBtn_Clicked(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
            else
            {
                Frame.Navigate(typeof(MainPage));
            }
        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var list = sender as ListView;
            var num = list.SelectedIndex;
            switch (num)
            {
                case 0:
                    Frame.Navigate(typeof(Pages.Setting.WelcomeSet));
                    break;
                case 1:
                    Frame.Navigate(typeof(Pages.Setting.SkinSetPage));
                    break;
                case 2:
                    Frame.Navigate(typeof(Pages.Setting.ListenQuSet));
                    break;
                case 3:
                    Frame.Navigate(typeof(Pages.Setting.DownloadQuSet));
                    break;
                case 4:
                    Frame.Navigate(typeof(Pages.Setting.AboutPage));
                    break;
                case 5:
                    var userInfo = await JyUserInfo.JyUserInfoManager.QuickLogin("e4e6005e3145b90b4edd99c0d0d35af9");
                    if (userInfo.isLoginSuccess)
                    {
                        feedback = new JyUserFeedback.JyUserFeedbackSDKManager();
                        JyUserFeedback.view.JyFeedbackControl.FeedbackImageRequested += JyFeedbackControl_FeedbackImageRequested;
                        feedback.ShowFeedBack(mainGrid, false, "e4e6005e3145b90b4edd99c0d0d35af9", userInfo.U_Key);
                    }
                    else
                    {
                        await new Windows.UI.Popups.MessageDialog(ResourceLoader.GetForCurrentView().GetString("FeedbackInitFalied")).ShowAsync();
                    }
                    break;
                default:
                    break;
            }
            list.SelectedIndex = -1;
        }

        private async void JyFeedbackControl_FeedbackImageRequested()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            var file= await picker.PickSingleFileAsync();
            if(file!=null)
            {
                feedback.UploadPicture("e4e6005e3145b90b4edd99c0d0d35af9", "b3a7acb3370d712073b4c26577bad19b", file);
            }
        }
    }
}
