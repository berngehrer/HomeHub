using openhab.net.rest;
using openhab.net.rest.Items;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace HomeHub
{
    enum SensorCommand : byte
    {
        Temperature = 0x11,
        Humidity = 0x12,
        GasLevel = 0x13,
        LuxLevel = 0x14
    };


    public sealed partial class MyUserControl1 : UserControl, IDisposable
    {
        const byte SLAVE_ADDRESS = 0x34;

        //byte[] _readBuffer;
        I2cDevice _device;
        ThreadPoolTimer _timer;

        ItemContext _context;
        NumberItem _modus;
        SwitchItem _sequence;
        DimmerItem _bridness;
        ColorItem _color;

        NumberItem _temp, hum, lux, ppm;

        public MyUserControl1()
        {
            InitializeComponent();

            //Loaded += (o, e) => ReadSensor().Wait();
            //_timer = ThreadPoolTimer.CreatePeriodicTimer(async _ => await ReadSensor(), TimeSpan.FromSeconds(30));
            GetItems();
        }

        async void GetItems()
        {
            _context = new ItemContext("192.168.178.69");

            _modus = await _context.GetByName<NumberItem>("Marco_Fountain_State");
            _sequence = await _context.GetByName<SwitchItem>("Marco_Fountain_Sequence");
            //_bridness = await _context.GetByName<DimmerItem>("Marco_Fountain_Bridness");
            _color = await _context.GetByName<ColorItem>("Marco_Fountain_Color");

            _modus.Changed += _modus_Changed;
            _sequence.Changed += _sequence_Changed;
            _color.Changed += _color_Changed;

            //_temp = await _context.GetByName<NumberItem>("Marco_Sensor1_Temp");
            //hum = await _context.GetByName<NumberItem>("Marco_Sensor1_Hum");
            //lux = await _context.GetByName<NumberItem>("Marco_Sensor1_Lux");
            //ppm = await _context.GetByName<NumberItem>("Marco_Sensor1_PPM");
        }

        async void _color_Changed(object sender, EventArgs e)
        {
            try
            {
                string selector = I2cDevice.GetDeviceSelector();
                var i2cSettings = new I2cConnectionSettings(0x33);
                var devices = await DeviceInformation.FindAllAsync(selector);
                
                using (_device = await I2cDevice.FromIdAsync(devices[0].Id, i2cSettings))
                {
                    _device.Write(new byte[] { 0x11, 0xFF, 0x00, 0x00 });
                    _device.Write(new byte[] { 0x12, 0x00, 0xFF, 0x00 });
                    _device.Write(new byte[] { 0x13, 0x00, 0x00, 0xFF });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: {0}", ex.Message);
                return;
            }
        }

        async void _sequence_Changed(object sender, EventArgs e)
        {
            try
            {
                string selector = I2cDevice.GetDeviceSelector();
                var i2cSettings = new I2cConnectionSettings(0x33);
                var devices = await DeviceInformation.FindAllAsync(selector);

                using (_device = await I2cDevice.FromIdAsync(devices[0].Id, i2cSettings))
                {
                    _device.Write(new byte[] { 0x02, (byte)(_sequence.Value ? 0x01 : 0x00) });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: {0}", ex.Message);
                return;
            }
        }

        async void _modus_Changed(object sender, EventArgs e)
        {
            try
            {
                string selector = I2cDevice.GetDeviceSelector();
                var i2cSettings = new I2cConnectionSettings(0x33);
                var devices = await DeviceInformation.FindAllAsync(selector);

                using (_device = await I2cDevice.FromIdAsync(devices[0].Id, i2cSettings))
                {
                    Modus modus = Modus.OFF;

                    switch ((int)_modus.Value)
                    {
                        case 0:
                            modus = Modus.OFF;
                            break;
                        case 1:
                            modus = Modus.PUMP;
                            break;
                        case 2:
                            modus = Modus.LED;
                            break;
                        case 3:
                            modus = Modus.ON;
                            break;
                    }
                    _device.Write(new byte[] { 0x01, (byte)modus });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: {0}", ex.Message);
                return;
            }
        }

        async Task ReadSensor()
        {
            try
            {

                var i2cSettings = new I2cConnectionSettings(SLAVE_ADDRESS);
                //i2cSettings.BusSpeed = I2cBusSpeed.FastMode;
                string selector = I2cDevice.GetDeviceSelector();
                var devices = await DeviceInformation.FindAllAsync(selector);


                using (_device = await I2cDevice.FromIdAsync(devices[0].Id, i2cSettings))
                {
                    _temp.Value = ReadAsFloat((byte)SensorCommand.Temperature);
                    hum.Value = ReadAsFloat((byte)SensorCommand.Humidity);
                    lux.Value = ReadAsInt16((byte)SensorCommand.LuxLevel);
                    ppm.Value = (float)GetPPM(ReadAsInt16((byte)SensorCommand.GasLevel));




                    //await GalaSoft.MvvmLight.Threading.DispatcherHelper.UIDispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, 
                    //    () =>
                    //    {
                    //        _temp.Value = 21.3f; // ReadAsFloat((byte)SensorCommand.Temperature);
                    //        hum.Value = ReadAsFloat((byte)SensorCommand.Humidity);
                    //        lux.Value = ReadAsInt16((byte)SensorCommand.LuxLevel);
                    //        ppm.Value = (float)GetPPM(ReadAsInt16((byte)SensorCommand.GasLevel));

                    //        txtTemp.Text = _temp.Value.ToString() + "°C";
                    //        txtHum.Text = hum.Value.ToString() + "%";
                    //        txtPPM.Text = ppm.Value.ToString() + " PPM";
                    //    }
                    //);
                    //GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() => { 
                    //    txtTemp.Text = ReadAsFloat((byte)SensorCommand.Temperature).ToString();
                    //    //
                    //});
                    //ReadAsFloat((byte)SensorCommand.Humidity);

                    //ReadAsInt16((byte)SensorCommand.GasLevel);
                    //ReadAsInt16((byte)SensorCommand.LuxLevel);
                }



                //var i2cSettings2 = new I2cConnectionSettings(0x33);
                //var devices2 = await DeviceInformation.FindAllAsync(selector);

                //using (_device = await I2cDevice.FromIdAsync(devices2[0].Id, i2cSettings2))
                //{
                //    byte[] buffer = new byte[1];
                //    _device.WriteRead(new byte[] { 0x03 }, buffer);
                //    await GalaSoft.MvvmLight.Threading.DispatcherHelper.UIDispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                //        () =>
                //        {
                //            txtWater.Text = ((int)(buffer[0] * 100 / 255.0f)).ToString() + "%";
                //        });
                //}
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: {0}", e.Message);
                return;
            }
            //finally
            //{
            //    _device = null;
            //}
        }

        short ReadAsInt16(byte cmd)
        {
            byte[] buffer = new byte[sizeof(short)];
            Read(cmd, ref buffer);
            return BitConverter.ToInt16(buffer, 0);
        }

        float ReadAsFloat(byte cmd)
        {
            byte[] buffer = new byte[sizeof(float)];
            Read(cmd, ref buffer);
            return BitConverter.ToSingle(buffer, 0);
        }

        void Read(byte cmd, ref byte[] buffer)
        {
            _device.WriteRead(new byte[] { cmd }, buffer);
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
            var resistance = ((1023.0 / (float)val) * 5.0 - 1.0) * RLOAD;
            return PARA * Math.Pow((resistance / RZERO), -PARB);
        }

        public void Dispose()
        {
            _context.Dispose();
            _timer?.Cancel();
        }

        bool isOn = true;
        async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selector = I2cDevice.GetDeviceSelector();
                var i2cSettings = new I2cConnectionSettings(0x33);
                var devices = await DeviceInformation.FindAllAsync(selector);

                using (_device = await I2cDevice.FromIdAsync(devices[0].Id, i2cSettings))
                {
                    if (isOn)
                    {
                        _device.Write(new byte[] { 0x01, 0x10 });
                    }
                    else
                    {
                        _device.Write(new byte[] { 0x01, 0x30 });
                    }
                    isOn = !isOn;
                    
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: {0}", ex.Message);
                return;
            }
        }
    }
}
