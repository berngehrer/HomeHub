using System;
using System.Threading.Tasks;
using Windows.Devices.I2c;

namespace SensorBox
{
    abstract class DeviceClient : IClient
    {
        I2cDevice _innerClient;

        public DeviceClient(DeviceBus bus, byte address)
        {
            Bus = bus; 
            Settings = new I2cConnectionSettings(address);
        }

        public DeviceBus Bus { get; }
        public I2cConnectionSettings Settings { get; }

        public bool IsConnected => _innerClient != null;


        public async Task<bool> Connect()
        {
            try {
                _innerClient = await I2cDevice.FromIdAsync(Bus.Id, Settings);
                return true;
            }
            catch { return false; }
        }

        public void Close() => Dispose();
        public void Dispose()
        {
            _innerClient?.Dispose();
            _innerClient = null;
        }


        protected void Write(byte[] bytes)
        {
            if (IsConnected) {
                try {
                    _innerClient.Write(bytes);
                } catch { _innerClient = null; }
            }
        }

        protected void Read(byte cmd, ref byte[] buffer)
        {
            if (IsConnected) {
                try {
                    _innerClient.WriteRead(new byte[] { cmd }, buffer);
                } catch { _innerClient = null; }
            }
        }

        protected short ReadAsInt16(byte cmd)
        {
            byte[] buffer = new byte[sizeof(short)];
            Read(cmd, ref buffer);
            return BitConverter.ToInt16(buffer, 0);
        }

        protected float ReadAsFloat(byte cmd)
        {
            byte[] buffer = new byte[sizeof(float)];
            Read(cmd, ref buffer);
            return BitConverter.ToSingle(buffer, 0);
        }
    }
}
