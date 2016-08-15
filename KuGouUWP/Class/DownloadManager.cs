using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.Xaml;

namespace KuGouUWP.Class
{
    public class DownloadManager
    {
        public enum DownloadType
        {
            song,mv,other
        }

        public enum DownloadResult
        {
            Success, Failure
        }

        public static async Task<ObservableCollection<DownloadModel>> GetDownload(DownloadType type)
        {
            var group= "";
            switch (type)
            {
                case DownloadType.song:
                    group = "song";
                    break;
                case DownloadType.mv:
                    group = "mv";
                    break;
                case DownloadType.other:
                    group = "other";
                    break;
                default:
                    break;
            }
            var downs = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(BackgroundTransferGroup.CreateGroup(group));
            var downlist = new ObservableCollection<DownloadModel>();
            if(downs.Count>0)
            {
                foreach (var down in downs)
                {
                    var model = new DownloadModel();
                    downlist.Add(model);
                }
            }
            return downlist;
        }

        public static async Task<bool> AddDownload(string filename, string url, DownloadType type, StorageFolder folder = null)
        {
            try
            {
                var down = new BackgroundDownloader();
                down.TransferGroup = BackgroundTransferGroup.CreateGroup("other");
                if (folder == null)
                {
                    folder = await KnownFolders.MusicLibrary.GetFolderAsync("kgdownload");
                    switch (type)
                    {
                        case DownloadType.song:
                            folder = await folder.GetFolderAsync("song");
                            down.TransferGroup = BackgroundTransferGroup.CreateGroup("song");
                            break;
                        case DownloadType.mv:
                            folder = await folder.GetFolderAsync("mv");
                            down.TransferGroup = BackgroundTransferGroup.CreateGroup("mv");
                            break;
                        case DownloadType.other:
                            folder = await folder.GetFolderAsync("other");
                            down.TransferGroup = BackgroundTransferGroup.CreateGroup("other");
                            break;
                        default:
                            break;
                    }
                }
                var files= (await folder.GetFilesAsync()).ToList();
                foreach (var item in files)
                {
                    await item.DeleteAsync();
                }
                var file = await folder.CreateFileAsync(filename);
                down.FailureToastNotification = DownloadToast(filename, DownloadResult.Failure);
                down.SuccessToastNotification = DownloadToast(filename, DownloadResult.Success);
                var opera= down.CreateDownload(new Uri(url), file);
                opera.CostPolicy = BackgroundTransferCostPolicy.Always;
                opera.StartAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static ToastNotification DownloadToast(string filename,DownloadResult result)
        {
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            switch (result)
            {
                case DownloadResult.Success:
                    toastTextElements[0].AppendChild(toastXml.CreateTextNode("下载完成!"));
                    toastTextElements[1].AppendChild(toastXml.CreateTextNode("文件:" + filename + "下载完成"));
                    break;
                case DownloadResult.Failure:
                    toastTextElements[0].AppendChild(toastXml.CreateTextNode("下载失败!"));
                    toastTextElements[1].AppendChild(toastXml.CreateTextNode("文件:" + filename + "下载失败"));
                    break;
                default:
                    break;
            }
            
            var doc = new Windows.Data.Xml.Dom.XmlDocument();
            ToastNotification toast = new ToastNotification(toastXml);
            return toast;
        }
        public class DownloadModel
        {
            public string filename { get; set; }
            public int process { get; set; }
            public string getsize { get; set; }
            public string allsize { get; set; }
            public Style style { get; set; }
            public string alert { get; set; }
            public string icon { get; set; }
            public DownloadOperation opera { get; set; }
            /*
            public DownloadModel(DownloadOperation op)
            {
                this.opera = op;
            }
            */
            public void Upload(ResourceDictionary Resources)
            {
                this.filename = opera.ResultFile.Name;
                switch (opera.Progress.Status)
                {
                    case BackgroundTransferStatus.Idle:
                        this.alert = "等待下载";
                        this.style = (Style)Resources["otherdownstyle"];
                        break;
                    case BackgroundTransferStatus.Running:
                        this.style = (Style)Resources["downloadingstyle"];
                        break;
                    case BackgroundTransferStatus.PausedByApplication:
                        this.style = (Style)Resources["downloadpasuestyle"];
                        break;
                    case BackgroundTransferStatus.PausedCostedNetwork:
                        this.style = (Style)Resources["downloadpasuestyle"];
                        break;
                    case BackgroundTransferStatus.PausedNoNetwork:
                        this.style = (Style)Resources["downloadpasuestyle"];
                        break;
                    case BackgroundTransferStatus.Completed:
                        this.alert = "下载完成";
                        this.style = (Style)Resources["otherdownstyle"];
                        break;
                    case BackgroundTransferStatus.Canceled:
                        this.alert = "下载取消";
                        this.style = (Style)Resources["otherdownstyle"];
                        break;
                    case BackgroundTransferStatus.Error:
                        this.alert = "下载错误";
                        this.style = (Style)Resources["otherdownstyle"];
                        break;
                    case BackgroundTransferStatus.PausedSystemPolicy:
                        this.alert = "下载挂起";
                        this.style = (Style)Resources["otherdownstyle"];
                        break;
                    default:
                        break;
                }
                if (opera.Progress.TotalBytesToReceive != 0 && opera.Progress.TotalBytesToReceive > 1048576)
                {
                    this.allsize = (opera.Progress.TotalBytesToReceive / 1048576).ToString("0.0");
                }
                if (opera.Progress.BytesReceived != 0 && opera.Progress.BytesReceived > 1048576)
                {
                    this.getsize = (opera.Progress.BytesReceived / 1048576).ToString("0.0");
                }
                if (opera.Progress.TotalBytesToReceive != 0 && opera.Progress.BytesReceived != 0)
                {
                    this.process = int.Parse(((Math.Floor(double.Parse(((opera.Progress.BytesReceived / opera.Progress.TotalBytesToReceive) * 100).ToString()))).ToString()));
                }
            }
        }
    }
}
