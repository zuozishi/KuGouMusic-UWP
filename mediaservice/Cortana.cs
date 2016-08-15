using System;
using System.Collections.Generic;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.VoiceCommands;

namespace mediaservice
{
    public sealed class Cortana : IBackgroundTask
    {
        /*
         VoiceCommandServiceConnection 类是接受Cortana传递过来的信息以及给Cortana回应信息的
        里面有几个重要的方法：
        GetVoiceCommandAsync        检索用户的语音命令提交Cortana通过语音或文本。
        ReportFailureAsync          发送一个响应，表明Cortana语音命令处理失败了。
        ReportProgressAsync         发送一个响应，Cortana在处理语音命令。
        ReportSuccessAsync          发送一个响应，Cortana语音命令已成功了。
        RequestAppLaunchAsync       发送一个响应，要求Cortana启动前台应用
        RequestConfirmationAsync    发送一个响应，指示Cortana语音命令需要确认。
        RequestDisambiguationAsync  发送一个响应，表示Cortana语音命令返回多个结果,需要用户选择一个。
        */
        BackgroundTaskDeferral _taskDerral;
        VoiceCommandServiceConnection _serviceConnection;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _taskDerral = taskInstance.GetDeferral();
            var details = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            // 验证是否调用了正确的app service
            if (details == null || details.Name != "CortanaService")
            {
                _taskDerral.Complete();
                return;
            }
            _serviceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(details);
            var cmd = await _serviceConnection.GetVoiceCommandAsync();
            _taskDerral.Complete();
        }
    }
}
