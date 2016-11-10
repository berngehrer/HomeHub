using System;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;

namespace FountainJob
{
    public sealed class StartupTask : IBackgroundTask
    {
        ThreadPoolTimer _updateTimer;
        BackgroundTaskDeferral _backgroundTaskDeferral;
        RuntimeManager _runtime = new RuntimeManager();
        
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _backgroundTaskDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;

            var asyncAction = ThreadPool.RunAsync(async _ => await _runtime.StartWatch());
            _updateTimer = ThreadPoolTimer.CreatePeriodicTimer(_ => _runtime.UpdateWaterLevel(), TimeSpan.FromMinutes(1));
        }

        void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _runtime.Dispose();
            _updateTimer.Cancel();
            _backgroundTaskDeferral.Complete();
        }
    }
}
