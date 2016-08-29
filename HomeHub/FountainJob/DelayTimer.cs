using System;
using System.Threading;
using System.Threading.Tasks;

namespace FountainJob
{
    internal class DelayTimer : CancellationTokenSource, IDisposable 
    {
        TimeSpan _delay;

        public DelayTimer(TimeSpan delay)
        {
            _delay = delay;
        }

        public void Start(Action callback)
        {
            Task.Run(() =>
            {
                for (;;)
                {
                    if (IsCancellationRequested) {
                        break;
                    }
                    callback.Invoke();
                    Task.Delay(_delay, Token).Wait();
                }
            },
            Token);
        }

        public new void Dispose()
        {
            base.Cancel();
        }
    }
}
