using System.ComponentModel;
using Windows.Networking.BackgroundTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using KuGouMusicUWP;
using KuGouMusicUWP.Pages;

namespace KG_ClassLibrary
{
    public class TransferModel : INotifyPropertyChanged
    {
        public DownloadOperation DownloadOperation { get; set; }
        public string Source { get; set; }
        public string filename
        {
            get
            {
                return DownloadOperation.ResultFile.Name;
            }
        }
        public string Destination { get; set; }

        private int _progress;
        public int Progress
        {
            get
            {
                return _progress;
            }
            set
            {
                _progress = value;
                if (Progress == 100)
                {
                    Upload();
                }
                RaisePropertyChanged("Progress");
            }
        }

        public string allsize
        {
            get
            {
                if(DownloadOperation.Progress.TotalBytesToReceive> 1048576)
                {
                    return (DownloadOperation.Progress.TotalBytesToReceive / 1048576).ToString("0.0");
                }else
                {
                    return "<1";
                }
            }
        }
        public string getsize
        {
            get
            {
                if (DownloadOperation.Progress.BytesReceived > 1048576)
                {
                    return (DownloadOperation.Progress.BytesReceived / 1048576).ToString("0.0");
                }
                else
                {
                    return "<1";
                }
            }
        }
        public string alert { get; set; }
        public Style style
        {
            get
            {
                switch (DownloadOperation.Progress.Status)
                {
                    case BackgroundTransferStatus.Idle:
                        this.alert = "等待下载";
                        return DownloadStyle.GetStyle(DownloadStyle.Type.Other);
                    case BackgroundTransferStatus.Running:
                        return DownloadStyle.GetStyle(DownloadStyle.Type.Downing);
                    case BackgroundTransferStatus.PausedByApplication:
                        return DownloadStyle.GetStyle(DownloadStyle.Type.Pause);
                    case BackgroundTransferStatus.PausedCostedNetwork:
                        return DownloadStyle.GetStyle(DownloadStyle.Type.Pause);
                    case BackgroundTransferStatus.PausedNoNetwork:
                        return DownloadStyle.GetStyle(DownloadStyle.Type.Pause);
                    case BackgroundTransferStatus.Completed:
                        this.alert = "下载完成";
                        return DownloadStyle.GetStyle(DownloadStyle.Type.Other);
                    case BackgroundTransferStatus.Canceled:
                        this.alert = "下载取消";
                        return DownloadStyle.GetStyle(DownloadStyle.Type.Other);
                    case BackgroundTransferStatus.Error:
                        this.alert = "下载错误";
                        return DownloadStyle.GetStyle(DownloadStyle.Type.Other);
                    case BackgroundTransferStatus.PausedSystemPolicy:
                        this.alert = "下载挂起";
                        return DownloadStyle.GetStyle(DownloadStyle.Type.Other);
                    default:
                        break;
                }
                return null;
            }
        }
        private ulong _totalBytesToReceive;
        public ulong TotalBytesToReceive
        {
            get
            {
                return _totalBytesToReceive;
            }
            set
            {
                _totalBytesToReceive = value;
                RaisePropertyChanged("TotalBytesToReceive");
            }
        }

        private ulong _bytesReceived;
        public ulong BytesReceived
        {
            get
            {
                return _bytesReceived;
            }
            set
            {
                _bytesReceived = value;
                RaisePropertyChanged("BytesReceived");
                RaisePropertyChanged("getsize");
            }
        }
        public void Upload()
        {
            RaisePropertyChanged("BytesReceived");
            RaisePropertyChanged("getsize");
            RaisePropertyChanged("TotalBytesToReceive");
            RaisePropertyChanged("Progress");
            RaisePropertyChanged("style");
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class DownloadStyle
    {
        public enum Type
        {
            Downing,Pause,Other
        }
        public static Style GetStyle(Type type)
        {
            var styletxt = "";
            switch (type)
            {
                case Type.Downing:
                    styletxt = downing;
                    break;
                case Type.Pause:
                    styletxt = pause;
                    break;
                case Type.Other:
                    styletxt = other;
                    break;
                default:
                    break;
            }
            return (Style)XamlReader.Load(styletxt);
        }
        private static string downing = @"<Style xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='ListViewItem'>
            <Setter Property='HorizontalContentAlignment' Value='Stretch'></Setter>
            <Setter Property='BorderBrush' Value='#7F808080'/>
            <Setter Property='Margin' Value='0'/>
            <Setter Property='Content'>
                <Setter.Value>
                    <Grid Height='55' Background='White' Margin='0,2,0,2'>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width='50'/>
                            <ColumnDefinition />
                            <ColumnDefinition Width='50'/>
                        </Grid.ColumnDefinitions>
                        <Grid Grid.Column='0'>
                            <AppBarButton Width='50' Height='50' Icon='Pause'></AppBarButton>
                        </Grid>
                        <Grid Grid.Column='1'>
                            <Grid.RowDefinitions>
                                <RowDefinition/>
                                <RowDefinition/>
                            </Grid.RowDefinitions>
                            <TextBlock Grid.Row='0' HorizontalAlignment='Left' VerticalAlignment='Bottom' Text='{Binding filename}'></TextBlock>
                            <TextBlock Grid.Row='0' HorizontalAlignment='Right' VerticalAlignment='Bottom' FontSize='13' Foreground='#FF767676'><Run Text='{Binding getsize}'></Run>M/<Run Text='{Binding allsize}'></Run>M</TextBlock>
                            <ProgressBar Value='{Binding Progress}' Grid.Row='1' VerticalAlignment='Center'></ProgressBar>
                        </Grid>
                        <Grid Grid.Column='2'>
                            <AppBarButton Width='50' Height='50' Icon='Delete'></AppBarButton>
                        </Grid>
                    </Grid>
                </Setter.Value>
            </Setter>
        </Style>";
        private static string pause= @"<Style xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='ListViewItem'>
            <Setter Property='HorizontalContentAlignment' Value='Stretch'></Setter>
            <Setter Property='BorderBrush' Value='#7F808080'/>
            <Setter Property='Margin' Value='0'/>
            <Setter Property='Content'>
                <Setter.Value>
                    <Grid Height='55' Background='White' Margin='0,2,0,2'>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width='50'/>
                            <ColumnDefinition />
                            <ColumnDefinition Width='50'/>
                        </Grid.ColumnDefinitions>
                        <Grid Grid.Column='0'>
                            <AppBarButton Width='50' Height='50' Icon='Play'></AppBarButton>
                        </Grid>
                        <Grid Grid.Column='1'>
                            <Grid.RowDefinitions>
                                <RowDefinition/>
                                <RowDefinition/>
                            </Grid.RowDefinitions>
                            <TextBlock Grid.Row='0' HorizontalAlignment='Left' VerticalAlignment='Center' Text='{Binding filename}'></TextBlock>
                            <TextBlock Grid.Row='0' HorizontalAlignment='Right' VerticalAlignment='Bottom' FontSize='13' Foreground='#FF767676'><Run Text='{Binding getsize}'></Run>M/<Run Text='{Binding allsize}'></Run>M</TextBlock>
                            <ProgressBar Value='{Binding Progress}' Grid.Row='1' VerticalAlignment='Top'></ProgressBar>
                            <TextBlock Grid.Row='1' FontSize='12' HorizontalAlignment='Left' VerticalAlignment='Center' Foreground='#FF838383'>点击继续下载</TextBlock>
                        </Grid>
                        <Grid Grid.Column='2'>
                            <AppBarButton Width='50' Height='50' Icon='Delete'></AppBarButton>
                        </Grid>
                    </Grid>
                </Setter.Value>
            </Setter>
        </Style>";
        private static string other = @"<Style xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' TargetType='ListViewItem'>
            <Setter Property='HorizontalContentAlignment' Value='Stretch'></Setter>
            <Setter Property='BorderBrush' Value='#7F808080'/>
            <Setter Property='Margin' Value='0'/>
            <Setter Property='Content'>
                <Setter.Value>
                    <Grid Height='55' Background='White' Margin='0,2,0,2'>
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition />
                            <ColumnDefinition Width='50'/>
                        </Grid.ColumnDefinitions>
                        <Grid Margin='15,0,0,0' Grid.Column='0'>
                            <Grid.RowDefinitions>
                                <RowDefinition/>
                                <RowDefinition/>
                            </Grid.RowDefinitions>
                            <TextBlock Grid.Row='0' HorizontalAlignment='Left' VerticalAlignment='Center' Text='{Binding filename}'></TextBlock>
                            <TextBlock Grid.Row='1' FontSize='12' HorizontalAlignment='Left' VerticalAlignment='Top' Foreground='#FF838383' Text='{Binding alert}'></TextBlock>
                        </Grid>
                        <Grid Grid.Column='1'>
                            <AppBarButton Width='50' Height='50' Icon='Delete'></AppBarButton>
                        </Grid>
                    </Grid>
                </Setter.Value>
            </Setter>
        </Style>";
    }
}
