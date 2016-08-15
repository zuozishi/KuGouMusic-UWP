using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace KG_ClassLibrary
{
    public class DownloadedToast
    {
        public enum DownResult
        {
            Su,Fa
        }
        /// <summary>
        /// 一首歌曲下载好后创建通知
        /// </summary>
        public static ToastNotification Done(string filename,DownResult res)
        {
            string result = "";
            switch (res)
            {
                case DownResult.Su:result="下载成功"; break;
                case DownResult.Fa: result = "下载失败"; break;
                default:break;
            }
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02);
            XmlNodeList elements = toastXml.GetElementsByTagName("text");
            elements[0].AppendChild(toastXml.CreateTextNode("酷狗音乐UWP"));
            elements[1].AppendChild(toastXml.CreateTextNode("文件:"+filename+result));
            ToastNotification toast = new ToastNotification(toastXml);
            return toast;
        }
    }
}
