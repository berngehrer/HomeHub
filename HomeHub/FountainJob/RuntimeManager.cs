using openhab.net.rest;
using openhab.net.rest.Items;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace FountainJob
{
    class RuntimeManager : CancellationTokenSource, IDisposable
    {
        I2cDevice _device;
        ItemContext _context;
        NumberItem _waterlevelItem;
        Object _asyncLock = new Object();

        readonly I2cConnectionSettings _settings = new I2cConnectionSettings(Constants.SLAVE_ADDRESS);
        

        public async Task StartWatch()
        {
            _context = new ItemContext(Constants.Openhab_Host);
            _context.Refreshed += Context_Refreshed;
            
            _waterlevelItem = await _context.GetByName<NumberItem>(Constants.Item_Waterlevel, Token);
            
            var devices = await DeviceInformation.FindAllAsync(I2cDevice.GetDeviceSelector());
            _device = await I2cDevice.FromIdAsync(devices[0].Id, _settings);
        }

        public void UpdateWaterLevel()
        {
            try
            {
                lock (_asyncLock) {
                    byte[] buffer = new byte[1];
                    _device.WriteRead(new byte[] { Constants.CMD_WATERLEVEL }, buffer);
                    _waterlevelItem.Value = (int)(buffer[0] * 100 / 255.0f);
                }
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.Fail(ex.Message);
            }
        }

        void Context_Refreshed(object sender, ContextRefreshedEventArgs<OpenhabItem> args)
        {
            lock (_asyncLock)
            switch (args.Element.Name)
            {
                case Constants.Item_State:
                    SetModus(args.Element as NumberItem);
                    break;
                case Constants.Item_Sequence:
                    SetSequence(args.Element as SwitchItem);
                    break;
                case Constants.Item_Bridness:
                    SetBridness(args.Element as DimmerItem);
                    break;
                case Constants.Item_Color:
                    SetColor(args.Element as ColorItem);
                    break;
            }
        }

        void SetModus(NumberItem item)
        {
            Modus modus;
            switch ((int)item?.Value)
            {
                case 1:  modus = Modus.PUMP; break;
                case 2:  modus = Modus.LED;  break;
                case 3:  modus = Modus.ON;   break;
                default: modus = Modus.OFF;  break;
            }
            Write(Constants.CMD_MODUS, (byte)modus);
        }

        void SetSequence(SwitchItem item)
        {
            if (item != null) {
                byte flag = item.Value ? Constants.TRUE : Constants.FALSE;
                Write(Constants.CMD_SEQUENCE, flag);
            }
        }

        void SetBridness(DimmerItem item)
        {
            if (item != null) {
                Write(Constants.CMD_BRIDNESS, (byte)item.Value);
            }
        }

        void SetColor(ColorItem item)
        {
            // Expand later for each light
            if (item != null) {
                byte[] color = { (byte)item.Red, (byte)item.Green, (byte)item.Blue };
                Write(Constants.CMD_LED1, color);
                Write(Constants.CMD_LED2, color);
                Write(Constants.CMD_LED3, color);
            }
        }

        void Write(byte cmd, params byte[] data)
        {
            try {
                _device.Write(new byte[] { cmd }.Concat(data).ToArray());
            }
            catch (Exception ex) {
                System.Diagnostics.Debug.Fail(ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                this.Cancel(false);
                _context.Dispose();
                _device.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
