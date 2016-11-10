using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace SensorBox
{
    sealed class DeviceBus
    {
        static DeviceBus _instance;
        public static DeviceBus Default => _instance ?? (_instance = new DeviceBus());


        private DeviceBus()
        { }

        public DeviceBus(string id)
        {
            Id = id;
        }

        public string Id { get; private set; }
        
        public async Task Initialize()
        {
            if (string.IsNullOrEmpty(Id))
            {
                var dic = await DeviceInformation.FindAllAsync( I2cDevice.GetDeviceSelector() );
                Id = dic.Where(x => x.IsEnabled).FirstOrDefault()?.Id;
            }
        }
    }
}
