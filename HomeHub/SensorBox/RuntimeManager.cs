using System;
using System.Threading;
using System.Threading.Tasks;

namespace SensorBox
{
    class RuntimeManager : CancellationTokenSource
    {
        readonly TimeSpan DelayTime = TimeSpan.FromSeconds(10);

        SensorDevice _sensor = new SensorDevice( 0x34 );
        MqttPublisher _mqttPub = new MqttPublisher( "192.168.178.69" );
        
                
        public async Task Start()
        {
            await DeviceBus.Default.Initialize();

            while (!IsCancellationRequested)
            {
                if (await CheckClient(_sensor) && await CheckClient(_mqttPub))
                {
                    try
                    {
                        await SendTemperature();
                        await SendHumidity();
                        await SendGas();
                        await SendLux();
                    }
                    catch { }
                }
                await Task.Delay(DelayTime);
            }
        }

        async Task<bool> CheckClient(IClient client)
        {
            if (!client.IsConnected) {
                return await client.Connect();
            }
            return true;
        }

        public void Stop()
        {
            Cancel(false);
            _sensor.Dispose();
            _mqttPub.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing) {
                Stop();
            }
        }




        async Task SendTemperature()
        {
            var temp = _sensor.GetTemperature();
            if (temp > 0) {
                var msg = string.Format("{0:0.00}", temp).Replace(',', '.');
                await _mqttPub.Send("/openhab/marco/room/temp", msg);
            }
        }

        async Task SendHumidity()
        {
            var hum = _sensor.GetHumidity();
            if (hum > 0) {
                var msg = ((int)Math.Round(hum)).ToString();
                await _mqttPub.Send("/openhab/marco/room/hum", msg);
            }
        }

        async Task SendGas()
        {
            var gas = _sensor.GetGas();
            if (gas > 0) {
                var msg = gas.ToString();
                await _mqttPub.Send("/openhab/marco/room/gas", msg);

                var ppm = GetPPM(gas);
                msg = ((int)Math.Round(ppm)).ToString();
                await _mqttPub.Send("/openhab/marco/room/ppm", msg);
            }
        }

        async Task SendLux()
        {
            var lux = _sensor.GetLux();
            if (lux > 0) {
                var percent = Math.Round(lux * 100.0 / 1024.0);
                var msg = ((int)percent).ToString();
                await _mqttPub.Send("/openhab/marco/room/lux1", msg);
            }
        }


        /// The load resistance on the board
        const double RLOAD = 10.0f;
        /// Calibration resistance at atmospheric CO2 level
        const double RZERO = 1850.0f; //1120.0f;
        /// Parameters for calculating ppm of CO2 from sensor resistance
        const double PARA = 116.6020682f;
        const double PARB = 2.769034857f;

        double GetPPM(int val)
        {
            var resistance = ((1023.0/ (float)val) * 5.0 - 1.0) * RLOAD; 
            return PARA * Math.Pow((resistance / RZERO), -PARB);
        }
    }
}
