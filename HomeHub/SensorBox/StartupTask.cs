using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.System.Threading;

namespace SensorBox
{
    public sealed class StartupTask : IBackgroundTask
    {
        MqttPublisher _mqttPub = new MqttPublisher("192.168.178.69");
        //ThreadPoolTimer _timer;        
        BackgroundTaskDeferral _deferral;

        RuntimeManager _runtime = new RuntimeManager();

        int index = 0;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;

            //_timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromSeconds(10));

            IAsyncAction asyncAction = ThreadPool.RunAsync(async hdl =>
            {
                //await _mqttPub.Connect(); 
                await _runtime.Start();       
                //while (hdl.Status != AsyncStatus.Canceled)
                //{
                //    await _mqttPub.Send("/openhab/marco/room/gas", (index++).ToString());
                //    await Task.Delay(TimeSpan.FromSeconds(10));
                //}
            });
        }

        //async void Timer_Tick(ThreadPoolTimer timer) => await _runtime.Loop();

        void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            //_timer.Cancel();
            _mqttPub.Dispose();
            //_runtime?.Stop();
            _deferral.Complete();
        }
    }
}
