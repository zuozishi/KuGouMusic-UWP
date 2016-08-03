using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Popups;

namespace KG_ClassLibrary
{
    public class BackgroundDownload
    {
        private static List<DownloadOperation> activeDownloads;
        public static ObservableCollection<TransferModel> transfers = new ObservableCollection<TransferModel>();
        private static CancellationTokenSource cancelToken = new CancellationTokenSource();
        public enum DownloadType
        {
            song,mv,other
        }
        public class ResultData
        {
            public ObservableCollection<TransferModel> transfers { get; set; }
            public List<StorageFile> files { get; set; }
        }
        /// <summary>
        /// 要下载调用这个方法
        /// </summary>
        /// <param name="url">下载的文件网址的来源</param>
        /// <returns></returns>
        public static async Task Start(string filename,string url,DownloadType type,StorageFolder folder=null)
        {
            try
            {
                Uri uri = new Uri(Uri.EscapeUriString(url), UriKind.RelativeOrAbsolute);
                BackgroundDownloader downloader = new BackgroundDownloader();
                string extname = "";
                if (folder==null)
                {
                    folder = await KnownFolders.MusicLibrary.CreateFolderAsync("kgdownload", CreationCollisionOption.OpenIfExists);
                    switch (type)
                    {
                        case DownloadType.song:
                            switch (KuGouMusicUWP.Class.Setting.DownQu.GetType())
                            {
                                case KuGouMusicUWP.Class.Setting.DownQu.Type.low:
                                    extname = ".mp3";
                                    break;
                                case KuGouMusicUWP.Class.Setting.DownQu.Type.mid:
                                    extname = ".mp3";
                                    break;
                                case KuGouMusicUWP.Class.Setting.DownQu.Type.high:
                                    extname = ".flac";
                                    break;
                                default:
                                    break;
                            }
                            folder = await folder.CreateFolderAsync("song", CreationCollisionOption.OpenIfExists);
                            downloader.TransferGroup = BackgroundTransferGroup.CreateGroup("song");
                            break;
                        case DownloadType.mv:
                            extname = ".mp4";
                            folder = await folder.CreateFolderAsync("mv", CreationCollisionOption.OpenIfExists);
                            downloader.TransferGroup = BackgroundTransferGroup.CreateGroup("mv");
                            break;
                        case DownloadType.other:
                            folder = await folder.CreateFolderAsync("other", CreationCollisionOption.OpenIfExists);
                            downloader.TransferGroup = BackgroundTransferGroup.CreateGroup("other");
                            break;
                        default:
                            break;
                    }
                }else
                {
                    downloader.TransferGroup = BackgroundTransferGroup.CreateGroup("other");
                }
                //string name = uri.ToString().Substring(uri.ToString().LastIndexOf("/"), uri.ToString().Length);
                string name = filename + extname; ;
                StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
                downloader.FailureToastNotification = DownloadedToast.Done(filename, DownloadedToast.DownResult.Fa);
                downloader.SuccessToastNotification = DownloadedToast.Done(filename, DownloadedToast.DownResult.Su);
                var download = downloader.CreateDownload(new Uri(url), file);
                TransferModel transfer = new TransferModel();
                transfer.DownloadOperation = download;
                transfer.Source = download.RequestedUri.ToString();
                transfer.Destination = download.ResultFile.Path;
                transfer.BytesReceived = download.Progress.BytesReceived;
                transfer.TotalBytesToReceive = download.Progress.TotalBytesToReceive;
                transfer.Progress = 0;
                transfers.Add(transfer);
                Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                download.StartAsync().AsTask(cancelToken.Token, progressCallback);
                if(type!= DownloadType.song)
                {
                    await new MessageDialog(ResourceLoader.GetForCurrentView().GetString("AddDownSuccess")).ShowAsync();
                }
            }
            catch
            {
                await new MessageDialog(ResourceLoader.GetForCurrentView().GetString("AddDownFalied")).ShowAsync();
            }
        }

        public void Cancel(bool all)//暂时只实现全部取消
        {
            try
            {
                //if (all)
                {
                    cancelToken.Cancel();
                    cancelToken.Dispose();
                    cancelToken = new CancellationTokenSource();
                }
                //else{ }
            }
            catch { }
        }
        public void Pause(bool all)//暂时只实现全部暂停
        {
            try
            {
                //if (all)
                {
                    foreach (TransferModel transfer in transfers)
                    {
                        if (transfer.DownloadOperation.Progress.Status == BackgroundTransferStatus.Running)
                        {
                            transfer.DownloadOperation.Pause();
                        }
                    }
                }
                //else { }
            }
            catch { }
        }

        public void Resume(bool all)//暂时只实现全部恢复
        {
            try
            {
                //if (all)
                {
                    foreach (TransferModel transfer in transfers)
                    {
                        if (transfer.DownloadOperation.Progress.Status == BackgroundTransferStatus.PausedByApplication)
                        {
                            transfer.DownloadOperation.Resume();
                        }
                    }
                }
                //else{ }
            }
            catch { }
        }
        /// <summary>
        /// 获取文件夹已经下载的文件信息
        /// </summary>
        /// <returns></returns>
        public static async Task<ResultData> GetList(DownloadType type)
        {
            try
            {
                IReadOnlyList<StorageFile> files = null;
                StorageFolder folder = null;
                folder = await KnownFolders.MusicLibrary.CreateFolderAsync("kgdownload", CreationCollisionOption.OpenIfExists);
                switch (type)
                {
                    case DownloadType.song:
                        folder = await folder.CreateFolderAsync("song", CreationCollisionOption.OpenIfExists);
                        break;
                    case DownloadType.mv:
                        folder = await folder.CreateFolderAsync("mv", CreationCollisionOption.OpenIfExists);
                        break;
                    case DownloadType.other:
                        folder = await folder.CreateFolderAsync("other", CreationCollisionOption.OpenIfExists);
                        break;
                    default:
                        break;
                }
                files = await folder.GetFilesAsync();                
                //DataConnect.transfersModel = transfers;//未下载成功的文件Item
                //DataConnect.filesModel = files;//下载成功的文件Item
                //await DiscoverActiveDownloadsAsync(type);
                var result = new ResultData();
                result.transfers = new ObservableCollection<TransferModel>();
                result.files = files.ToList();
                if(transfers.Count>0)
                {
                    foreach (var item in transfers)
                    {
                        string flag = "";
                        switch (type)
                        {
                            case DownloadType.song:
                                flag = "song";
                                break;
                            case DownloadType.mv:
                                flag = "mv";
                                break;
                            case DownloadType.other:
                                flag = "other";
                                break;
                            default:
                                break;
                        }
                        if(item.DownloadOperation.TransferGroup.Name==flag)
                        {
                            result.transfers.Add(item);
                        }
                    }
                }
                return result;
            }
            catch { return null; }
        }

        /// <summary>
        /// 设置下载进度的信息
        /// </summary>
        /// <param name="download"></param>
        private static void DownloadProgress(DownloadOperation download)
        {
            try
            {
                TransferModel transfer = transfers.First(p => p.DownloadOperation == download);
                transfer.Progress = (int)((download.Progress.BytesReceived * 100) / download.Progress.TotalBytesToReceive);
                transfer.BytesReceived = download.Progress.BytesReceived;
                transfer.TotalBytesToReceive = download.Progress.TotalBytesToReceive;
            }
            catch { }
        }
    }
}
