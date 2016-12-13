using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace I2CLcd
{
    public sealed class I2CLcd : IDisposable
    {

        #region Cmd & Flags

        // Commands
        const byte LCD_CLEARDISPLAY         = 0x01;
        const byte LCD_RETURNHOME           = 0x02;
        const byte LCD_ENTRYMODESET         = 0x04;
        const byte LCD_DISPLAYCONTROL       = 0x08;
        const byte LCD_CURSORSHIFT          = 0x10;
        const byte LCD_FUNCTIONSET          = 0x20;
        const byte LCD_SETCGRAMADDR         = 0x40;
        const byte LCD_SETDDRAMADDR         = 0x80;
        
        // Display function flags
        const byte LCD_4BITMODE             = 0x00;
        const byte LCD_2LINE                = 0x08;
        const byte LCD_5x8DOTS              = 0x00;

        // Display control flags
        const byte LCD_DISPLAYON            = 0x04;
        //const byte LCD_CURSORON           = 0x02;
        const byte LCD_CURSOROFF            = 0x00;
        //const byte LCD_BLINKON            = 0x01;
        const byte LCD_BLINKOFF             = 0x00;

        // Display entry mode flags
        const byte LCD_ENTRYLEFT            = 0x02;
        const byte LCD_ENTRYSHIFTDECREMENT  = 0x00;

        // Backlight flags
        const byte LCD_BACKLIGHTON          = 0x08;
        const byte LCD_NOBACKLIGHTOFF       = 0x00;

        #endregion


        #region Instance Variables

        I2cDevice _deviceExpander;
        byte _backlight = LCD_BACKLIGHTON;

        byte[] _rowOffsets = { 0x00, 0x40, 0x14, 0x54 };

        #endregion


        #region Ctor

        public I2CLcd(byte address)
        {
            Task.Run(() => StartI2C(address)).Wait();
            InitLcd();
        }

        #endregion


        #region Initialization

        async Task StartI2C(byte deviceAddress)
        {
            try
            {
                var i2cSettings = new I2cConnectionSettings(deviceAddress);
                i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
                string deviceSelector = I2cDevice.GetDeviceSelector();
                var i2cDeviceControllers = await DeviceInformation.FindAllAsync(deviceSelector);
                _deviceExpander = await I2cDevice.FromIdAsync(i2cDeviceControllers[0].Id, i2cSettings);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }
        
        void InitLcd()
        {
            // According to datasheet, we need at least 40ms after 
            // power rises above 2.7V before sending commands.
            Task.Delay(50).Wait();

            // Init sequence 
            Write4Bits(0x03 << 4);
            DelayMicro(4500); 
            Write4Bits(0x03 << 4);
            DelayMicro(4500); 
            Write4Bits(0x03 << 4);
            DelayMicro(150);
            // Set 4-bit mode
            Write4Bits(0x02 << 4);

            // Set function set
            Command(Convert.ToByte(LCD_FUNCTIONSET | LCD_4BITMODE | LCD_2LINE | LCD_5x8DOTS));
            // Set display control
            Command(Convert.ToByte(LCD_DISPLAYCONTROL | LCD_DISPLAYON | LCD_CURSOROFF | LCD_BLINKOFF));

            // Clear display
            Clear();            
            // Set display mode
            Command(Convert.ToByte(LCD_ENTRYMODESET | LCD_ENTRYLEFT | LCD_ENTRYSHIFTDECREMENT));
            // Reset cursor
            Home();  
        }

        #endregion


        #region Public Control Functions

        public void Home()
        {
            Command(LCD_RETURNHOME);
            DelayMicro(2000);
        }

        public void Clear()
        {
            Command(LCD_CLEARDISPLAY);
            DelayMicro(2000);
        }

        public void BacklightOn()
        {
            _backlight = LCD_BACKLIGHTON;
            ExpanderWrite(0x00);
        }

        public void BacklightOff()
        {
            _backlight = LCD_NOBACKLIGHTOFF;
            ExpanderWrite(0x00);
        }
        
        public void SetCursor(byte row, byte col)
        {
            Command(Convert.ToByte(LCD_SETDDRAMADDR | (col + _rowOffsets[row])));
        }
        
        public void CreateChar(byte address, [ReadOnlyArray] byte[] charmap)
        {
            address &= 0x7;
            Command(Convert.ToByte(LCD_SETCGRAMADDR | (address << 3)));
            for (int i = 0; i < 8; i++) {
                Write(charmap[i]);
            }
        }

        public void Write(byte data) => Send(data, 0x01);
        public void Command(byte value) => Send(value, 0x00);
        
        #endregion


        #region Private Expander Functions

        void Send(byte value, byte mode)
        {
            int highnib = value & 0xF0;
            int lownib = (value << 4) & 0xF0;
            Write4Bits(Convert.ToByte(highnib | mode));
            Write4Bits(Convert.ToByte(lownib | mode));
        }

        void Write4Bits(byte value)
        {
            ExpanderWrite(value);
            PulseEnable(value);
        }

        void PulseEnable(byte data)
        {
            ExpanderWrite(data | 0x04);  // En high
            ExpanderWrite(data & ~0x04); // En low
            DelayMicro(50);             
        }
        
        void ExpanderWrite(int data)
        {
            _deviceExpander?.Write(new byte[] { Convert.ToByte(data | _backlight) });
        }
                
        void DelayMicro(int micro)
        {
            Task.Delay(TimeSpan.FromMilliseconds(micro / 1000.0)).Wait();
        }

        #endregion


        #region IDisposable

        public void Dispose() => _deviceExpander.Dispose();

        #endregion

    }


    public static class I2CLcdExtensions
    {
        public static void Write(this I2CLcd lcd, string s)
        {
            foreach (var c in s.ToCharArray()) {
                lcd.Write((byte)c);
            }
        }
    }
}
