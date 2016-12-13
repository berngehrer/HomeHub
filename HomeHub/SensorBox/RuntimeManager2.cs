using openhab.net.rest;
using openhab.net.rest.Items;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace SensorBox
{
    class RuntimeManager2 : CancellationTokenSource, IDisposable
    {
        /// The load resistance on the board
        const double RLOAD = 10.0f;
        /// Calibration resistance at atmospheric CO2 level
        const double RZERO = 1850.0f; //1120.0f;
        /// Parameters for calculating ppm of CO2 from sensor resistance
        const double PARA = 116.6020682f;
        const double PARB = 2.769034857f;


        I2cDevice _device;
        ItemContext _context;
        NumberItem _tempItem, _humItem, _ppmItem, _luxItem;

        Object _asyncLock = new Object();

        readonly I2cConnectionSettings _settings = new I2cConnectionSettings(Constants.SLAVE_ADDRESS);


        public async Task Start()
        {
            _context = new ItemContext(Constants.Openhab_Host);

            _tempItem = await _context.GetByName<NumberItem>(Constants.Item_Temp, Token);
            _humItem = await _context.GetByName<NumberItem>(Constants.Item_Hum, Token);
            _ppmItem = await _context.GetByName<NumberItem>(Constants.Item_PPM, Token);
            _luxItem = await _context.GetByName<NumberItem>(Constants.Item_Lux, Token);

            var devices = await DeviceInformation.FindAllAsync(I2cDevice.GetDeviceSelector());
            _device = await I2cDevice.FromIdAsync(devices[0].Id, _settings);
        }

        public void UpdateSensor()
        {
            try
            {
                lock (_asyncLock) {
                    _tempItem.Value = ReadAsFloat(Constants.CMD_TEMP);
                    _humItem.Value = ReadAsFloat(Constants.CMD_HUM);
                    _ppmItem.Value = (int)GetPPM();
                    _luxItem.Value = (int)GetLux();
                }
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.Fail(ex.Message);
            }
        }

        double GetPPM()
        {
            var gas = ReadAsInt16(Constants.CMD_GAS);
            var resistance = ((1023.0 / gas) * 5.0 - 1.0) * RLOAD;
            return PARA * Math.Pow((resistance / RZERO), -PARB);
        }

        double GetLux()
        {
            // TODO: Get Lux from value
            return ReadAsInt16(Constants.CMD_LUX);
        }

        short ReadAsInt16(byte cmd)
        {
            byte[] buffer = new byte[sizeof(short)];
            Read(cmd, ref buffer);
            var value = BitConverter.ToInt16(buffer, 0);
            return (value >= 0) ? value : (short)0;
        }

        float ReadAsFloat(byte cmd)
        {
            byte[] buffer = new byte[sizeof(float)];
            Read(cmd, ref buffer);
            var value = BitConverter.ToSingle(buffer, 0);
            return !float.IsNaN(value) ? value : 0f;
        }

        void Read(byte cmd, ref byte[] buffer)
        {
            try {
                _device.WriteRead(new byte[] { cmd }, buffer);
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.Fail(ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Cancel(false);
                _context.Dispose();
                _device.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
