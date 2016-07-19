using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Notifications;

namespace mediaservice
{
    public sealed class TitleUpdateTask : IBackgroundTask
    {
            public async void Run(IBackgroundTaskInstance taskInstance)
        {
            //通知
            ApplicationDataContainer container = ApplicationData.Current.LocalSettings;
            var deferral = taskInstance.GetDeferral();
            await GetLatestNews();
            deferral.Complete();
        }

        private IAsyncOperation<string> GetLatestNews()
        {
            try
            {
                return AsyncInfo.Run(token => GetNews());
            }
            catch (Exception)
            {
                // ignored
            }
            return null;
        }
        private static async Task<NowPlay> GetNowPlay()
        {
            try
            {
                var datafolder = await ApplicationData.Current.LocalFolder.GetFolderAsync("Data");
                var nowplayfile = await datafolder.GetFileAsync("nowplay.json");
                var json = await Windows.Storage.FileIO.ReadTextAsync(nowplayfile);
                var obj = Windows.Data.Json.JsonObject.Parse(json);
                var nowplay = new NowPlay();
                nowplay.title = obj.GetNamedString("title");
                nowplay.singername = obj.GetNamedString("singername");
                nowplay.imgurl = obj.GetNamedString("imgurl");
                return nowplay;
            }
            catch (Exception)
            {
                return null;
            }
        }
        private async Task<string> GetNews()
        {
            try
            {
                var response = await GetNowPlay();

                if (response != null)
                {
                    //var news = response.Data.Take(5).ToList();
                    UpdatePrimaryTile(response);
                    //UpdateSecondaryTile(response);
                }

            }
            catch (Exception)
            {
                // ignored
            }
            return null;
        }
        private class NowPlay
        {
            public string title { get; set; }
            public string singername { get; set; }
            public string url { get; set; }
            public string imgurl { get; set; }
            public string albumid { get; set; }
            public List<string> pics { get; set; }
        }

        private void UpdatePrimaryTile(NowPlay nowplay)
        {
            try
            {
                var updater = TileUpdateManager.CreateTileUpdaterForApplication();
                updater.EnableNotificationQueueForWide310x150(true);
                updater.EnableNotificationQueueForSquare150x150(true);
                updater.EnableNotificationQueueForSquare310x310(true);
                updater.EnableNotificationQueue(true);
                updater.Clear();
                var doc = new Windows.Data.Xml.Dom.XmlDocument();
                string xml = "";
                /*
                if (nowplay.imgurl == null && nowplay.imgurl == "")
                {
                    xml = string.Format(TileTemplateXml2, nowplay.title, nowplay.singername, "这是歌词");
                }
                else
                {
                    xml = string.Format(TileTemplateXml1, nowplay.imgurl, nowplay.title, nowplay.singername, "这是歌词");
                }
                */
                xml = string.Format(TileTemplateXml,nowplay.imgurl, nowplay.title, nowplay.singername);
                doc.LoadXml(WebUtility.HtmlDecode(xml), new XmlLoadSettings
                {
                    ProhibitDtd = false,
                    ValidateOnParse = false,
                    ElementContentWhiteSpace = false,
                    ResolveExternals = false
                });
                updater.Update(new TileNotification(doc));
            }
            catch (Exception)
            {
                // ignored
            }
        }
        private const string TileTemplateXml = @"
<tile branding='name'> 
  <visual version='2'>
    <binding template='TileMedium'>
      <image src='{0}' placement='peek'/>
      <text hint-wrap='true'>{1}</text>
      <text id='2' hint-style='captionsubtle' hint-wrap='true'>{2}</text>
    </binding>
    <binding template='TileWide'>
      <image placement='peek' src='{0}'/>
      <text id='1'>{1}</text>
      <text id='2' hint-style='captionsubtle' hint-wrap='true'>{2}</text>
    </binding>
  </visual>
</tile>";
        private const string TileTemplateXml1 = @"
<tile branding='name'> 
  <visual version='2'>
    <binding template='TileMedium'>
      <image src='{0}' placement='peek'/>
      <text hint-wrap='true'>{1}</text>
      <text id='2' hint-style='captionsubtle' hint-wrap='true'>{2}</text>
      <text id='3' hint-style='captionsubtle' hint-wrap='true'>{3}</text>
    </binding>
    <binding template='TileWide'>
      <image placement='peek' src='{0}'/>
      <textt hint-wrap='true' id='1'>{1}</text>
      <text id='2' hint-style='captionsubtle' hint-wrap='true'>{2}</text>
      <text id='3' hint-style='captionsubtle' hint-wrap='true'>{3}</text>
    </binding>
  </visual>
</tile>";
        private const string TileTemplateXml2 = @"
<tile branding='name'> 
  <visual version='2'>
    <binding template='TileMedium'>
      <text hint-wrap='true'>{0}</text>
      <text id='2' hint-style='captionsubtle' hint-wrap='true'>{1}</text>
      <text id='3' hint-style='captionsubtle' hint-wrap='true'>{2}</text>
    </binding>
    <binding template='TileWide'>
      <textt hint-wrap='true' id='1'>{0}</text>
      <text id='2' hint-style='captionsubtle' hint-wrap='true'>{1}</text>
      <text id='3' hint-style='captionsubtle' hint-wrap='true'>{2}</text>
    </binding>
  </visual>
</tile>";
    }
}
