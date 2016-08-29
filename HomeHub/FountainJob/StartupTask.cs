using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.System.Threading;

namespace FountainJob
{
    public sealed class StartupTask : IBackgroundTask
    {
        RuntimeManager _runtime;
        BackgroundTaskDeferral _backgroundTaskDeferral;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _backgroundTaskDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += TaskInstance_Canceled;

            IAsyncAction asyncAction = ThreadPool.RunAsync((handler) =>
            {
                _runtime = new RuntimeManager();
                _runtime.Start();
            });
        }

        void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            _runtime?.Stop();
            _backgroundTaskDeferral.Complete();
        }
    }
}
