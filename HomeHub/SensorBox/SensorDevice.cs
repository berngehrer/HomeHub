
namespace SensorBox
{
    class SensorDevice : DeviceClient
    {
        enum SensorCommand : byte
        {
            Temperature = 0x11,
            Humidity    = 0x12,
            GasLevel    = 0x13,
            LuxLevel    = 0x14
        };

        public SensorDevice(byte address) : base(DeviceBus.Default, address)
        { 
        }


        public float GetTemperature() => ReadAsFloat( (byte)SensorCommand.Temperature );
        public float GetHumidity() => ReadAsFloat( (byte)SensorCommand.Humidity );

        public short GetGas() => ReadAsInt16( (byte)SensorCommand.GasLevel );
        public short GetLux() => ReadAsInt16( (byte)SensorCommand.LuxLevel );
    }
}
