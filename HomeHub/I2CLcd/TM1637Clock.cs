using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Gpio;

namespace I2CLcd
{
    public sealed class TM1637Clock
    {
        const byte MAX_BRIDNESS = 7;

        const byte TM1637_I2C_COMM1 = 0x40;
        const byte TM1637_I2C_COMM2 = 0xC0;
        const byte TM1637_I2C_COMM3 = 0x80;

        byte _bridness;
        bool _isActive = true;
        GpioPin _clockPin, _dataPin;

        readonly byte[] SEGMENTS = {
            0x3F,    // 0
            0x06,    // 1
            0x5B,    // 2
            0x4F,    // 3
            0x66,    // 4
            0x6D,    // 5
            0x7D,    // 6
            0x07,    // 7
            0x7F,    // 8
            0x6F,    // 9
            0x77,    // A
            0x7C,    // b
            0x39,    // C
            0x5E,    // d
            0x79,    // E
            0x71     // F
        };


        public TM1637Clock(byte clockPin, byte dataPin)
            : this(clockPin, dataPin, MAX_BRIDNESS)
        {
        }

        public TM1637Clock(byte clockPin, byte dataPin, byte bridness)
        {
            _bridness = bridness;

            var controller = GpioController.GetDefault();
            if (controller != null)
            {
                _clockPin = controller.OpenPin(clockPin);
                _clockPin.SetDriveMode(GpioPinDriveMode.Input);
                _clockPin.Write(GpioPinValue.Low);

                _dataPin = controller.OpenPin(dataPin);
                _dataPin.SetDriveMode(GpioPinDriveMode.Input);
                _dataPin.Write(GpioPinValue.Low);
            }
        }

        ~TM1637Clock()
        {
            _clockPin.Dispose();
            _dataPin.Dispose();
        }


        public void SetBridness(byte bridness)
        {
            _bridness = (byte)((bridness & 0x07) | (_isActive ? 0x08 : 0x00));
        }

        public void TurnOn()
        {
            _isActive = true;
            SetBridness(_bridness);
        }

        public void TurnOff()
        {
            _isActive = false;
            SetBridness(_bridness);
        }

        public void Clear()
        {
            ShowDigits(new byte[] { 0x7F, 0x7F, 0x7F, 0x7F }, 0);
        }

        public void ShowDigits([ReadOnlyArray] byte[] data, byte pos)
        {
            // Write COMM1
            Start();
            Write(TM1637_I2C_COMM1);
            Stop();

            // Write COMM2 + first digit address
            Start();
            Write((byte)(TM1637_I2C_COMM2 + (pos & 0x03)));
	        // Write the data bytes
	        for (byte i = 0; i < data.Length; i++)
                Write(SEGMENTS[i]);
            Stop();

            // Write COMM3 + brightness
            Start();
            Write((byte)(TM1637_I2C_COMM3 + (_bridness & 0x0F)));
            Stop();
        }

        void Start()
        {
            _dataPin.SetDriveMode(GpioPinDriveMode.Output);
            BitDelay();
        }

        void Stop()
        {
            _dataPin.SetDriveMode(GpioPinDriveMode.Output);
            BitDelay();
            _clockPin.SetDriveMode(GpioPinDriveMode.Input);
            BitDelay();
            _dataPin.SetDriveMode(GpioPinDriveMode.Input);
            BitDelay();
        }

        bool Write(byte data)
        {
            // 8 Data Bits
            for (int i = 0; i < 8; i++)
            {
                // CLK low
                _clockPin.SetDriveMode(GpioPinDriveMode.Output);
                BitDelay();

                // Set data bit
                if ((data & 0x01) > 0)
                    _dataPin.SetDriveMode(GpioPinDriveMode.Input);
                else
                    _dataPin.SetDriveMode(GpioPinDriveMode.Output);
                BitDelay();

                // CLK high
                _clockPin.SetDriveMode(GpioPinDriveMode.Input);
                BitDelay();
                data = (byte)(data >> 1);
            }

            // Wait for acknowledge
            // CLK to zero
            _clockPin.SetDriveMode(GpioPinDriveMode.Output);
            _dataPin.SetDriveMode(GpioPinDriveMode.Input);
            BitDelay();

            // CLK to high
            _clockPin.SetDriveMode(GpioPinDriveMode.Input);
            BitDelay();
            bool ack = _dataPin.Read() == GpioPinValue.High;
            if (ack)
                _dataPin.SetDriveMode(GpioPinDriveMode.Output);
            
            BitDelay();
            _clockPin.SetDriveMode(GpioPinDriveMode.Output);
            BitDelay();

            return ack;
        }

        //byte EncodeDigit(byte digit)
        //{
        //    return SEGMENTS[digit & 0x0F];
        //}

        void BitDelay()
        {
            Task.Delay(TimeSpan.FromMilliseconds(50 / 1000.0)).Wait();
        }
    }
}
