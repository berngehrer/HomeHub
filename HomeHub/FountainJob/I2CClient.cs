using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace FountainJob
{
    class I2CClient : IDisposable
    {
        I2cDevice _device;

        public I2CClient(byte slaveAddress)
        {
            Settings = new I2cConnectionSettings(slaveAddress);
        }

        public I2cConnectionSettings Settings { get; }


        public async Task<bool> Connect()
        {
            try
            {
                var selector = I2cDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(selector);
                _device = await I2cDevice.FromIdAsync(dis[0].Id, Settings);
                return true;
            }
            catch { }
            return false;
        }

        public void Ping()
        {
            try
            {
                _device.Write(new byte[] { Constants.CMD_REMOTE, 0x01 });
            }
            catch { }
        }

        public void SetModus(Modus modus)
        {
            try
            {
                _device.Write(new byte[] { Constants.CMD_MODUS, (byte)modus });
            }
            catch { }
        }

        public void Dispose()
        {
            _device?.Dispose();
        }
    }
}
