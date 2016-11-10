using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.System.Threading;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace I2CLcd
{
    public sealed class StartupTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;


        byte[] smiley = { 0x00, 0x11, 0x00, 0x04, 0x00, 0x11, 0x0E, 0x00 };

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            //taskInstance.Canceled += TaskInstance_Canceled;

            using (I2CLcd lcd = new I2CLcd(0x3F))
            {
                lcd.CreateChar(0x01, smiley);

                lcd.SetCursor(0, 3);
                lcd.Write("Hello, World!");
                lcd.SetCursor(1, 2);
                lcd.Write("Have a nice day!");

                lcd.SetCursor(2, 9);
                lcd.Write(0x01);

                lcd.SetCursor(3, 7);
                lcd.Write(DateTime.Now.ToString("HH:mm"));


                //System.Threading.Tasks.Task.Delay(10000).Wait();

                //lcd.Clear();
                //lcd.BacklightOff();
            }

            _deferral.Complete();

            //IAsyncAction asyncAction = ThreadPool.RunAsync(async hdl =>
            //{
            //    //await _mqttPub.Connect(); 
            //    await lcd.Begin();
            //    //while (hdl.Status != AsyncStatus.Canceled)
            //    //{
            //    //    await _mqttPub.Send("/openhab/marco/room/gas", (index++).ToString());
            //    //    await Task.Delay(TimeSpan.FromSeconds(10));
            //    //}
            //});
        }

        //async void Timer_Tick(ThreadPoolTimer timer) => await _runtime.Loop();

        //void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        //{
        //    _deferral.Complete();
        //}
    }
}
