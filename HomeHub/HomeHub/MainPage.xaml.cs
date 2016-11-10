using Windows.Devices.Enumeration;
using Windows.Devices.I2c;
using Windows.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using openhab.net.rest.Items;
using openhab.net.rest;

enum Modus
{
    OFF  = 0x00,
    PUMP = 0x10,
    LED  = 0x20,
    ON   = PUMP | LED
};

//interface II2CClient : IDisposable
//{
//    void Open();
//    void Close();

//    byte[] Get(byte cmd);
//    bool Set(byte cmd, params byte[] param);
//}

class I2CClient : IDisposable
{
    I2cDevice _device;

    public I2CClient(byte slaveAddress)
    {
        Settings = new I2cConnectionSettings(slaveAddress);
    }

    //public bool IsConnected { get; private set; }
    public I2cConnectionSettings Settings { get; }

    public async Task Connect()
    {
        var selector = I2cDevice.GetDeviceSelector();
        var dis = await DeviceInformation.FindAllAsync(selector);
        _device = await I2cDevice.FromIdAsync(dis[0].Id, Settings);
        //IsConnected = true;
    }

    //public void Close()
    //{
    //    Dispose();
    //    IsConnected = false;
    //}

    public byte[] Request(byte cmd, byte bufferSize = 1) 
    {
        byte[] readBuffer = new byte[bufferSize];
        _device.WriteRead(new byte[] { cmd }, readBuffer);
        return readBuffer;
    }

    public bool Send(byte cmd, params byte[] param)
    {
        byte[] readBuffer = new byte[param.Length];
        byte[] writeBuffer = new byte[param.Length + 1];
        writeBuffer[0] = cmd;
        param.CopyTo(writeBuffer, 1);
        _device.WriteRead(writeBuffer, readBuffer);
        return readBuffer.SequenceEqual(param);
    }

    public void Dispose()
    {
        _device?.Dispose();
    }
}

class WaterLightClient : I2CClient
{
    private const byte CMD_MODUS    = 0x01;
    private const byte CMD_REMOTE   = 0x02;
    private const byte CMD_LED1     = 0x11;
    private const byte CMD_LED2     = 0x12;
    private const byte CMD_LED3     = 0x13;
    private const byte CMD_BRIDNESS = 0x20;
    
    public WaterLightClient(byte slaveAddress) : base(slaveAddress)
    {
    }

    public Modus GetModus()
    {
        return (Modus)Request(CMD_MODUS).FirstOrDefault();
    }

    public bool SetModus(Modus modus)
    {
        return Send(CMD_MODUS, (byte)modus);
    }
}


namespace HomeHub
{
    public sealed partial class MainPage : Page, IDisposable
    {
        ItemContext _context;
        SwitchItem _tvled;
        byte[] i2CReadBuffer;

        private const byte SLAVE_ADDRESS    = 0x40;

        private const byte CMD_MODUS = 0x01;
        private const byte CMD_SEQUENCE = 0x02;
        private const byte CMD_WATERLEVEL = 0x03;
        private const byte CMD_LED1 = 0x11;
        private const byte CMD_LED2 = 0x12;
        private const byte CMD_LED3 = 0x13;
        private const byte CMD_BRIDNESS = 0x20;


        public MainPage()
        {
            this.InitializeComponent();

            //GetLed();
            //StartI2C();
        }

        async void GetLed()
        {
            _context = new ItemContext("192.168.178.69");
            _tvled = await _context.GetByName<SwitchItem>("MQTT_TVLED_POW");
            
        }
        

        async void StartI2C()
        {
            //using (var client = new WaterLightClient(SLAVE_ADDRESS))
            //{
            //    await client.Connect();

            //    var modus = client.GetModus();
            //    bool b1 = client.SetModus(Modus.PUMP); 
            //}

            //using (var client = new I2CClient(SLAVE_ADDRESS))
            //{
            //    await client.Connect();

            //    bool b1 = client.Send(CMD_MODUS, (byte)Modus.PUMP);
            //    bool b2 = client.Send(CMD_MODUS, (byte)Modus.LED);
            //    bool b3 = client.Send(CMD_MODUS, (byte)Modus.ON);

            //    var v1 = client.Request(CMD_REMOTE);
            //    var v2 = client.Request(CMD_LED1, 3);

            //    var b4 = client.Send(CMD_LED1, 0xFF, 0x00, 0x00);
            //}


            I2cDevice device;
            try
            {
                i2CReadBuffer = new byte[1];
                var i2cSettings = new I2cConnectionSettings(SLAVE_ADDRESS);

                string aqs = I2cDevice.GetDeviceSelector();

                var dis = await DeviceInformation.FindAllAsync(aqs);

                string id = dis[0].Id;

                device = await I2cDevice.FromIdAsync(id, i2cSettings);

                //device.WriteRead(new byte[] { CMD_MODUS, (byte)Modus.PUMP }, i2CReadBuffer);
                //device.WriteRead(new byte[] { CMD_MODUS, (byte)Modus.LED }, i2CReadBuffer);
                //device.WriteRead(new byte[] { CMD_MODUS, (byte)Modus.ON }, i2CReadBuffer);
                //device.WriteRead(new byte[] { CMD_REMOTE }, i2CReadBuffer);
                //device.WriteRead(new byte[] { CMD_LED1 }, i2CReadBuffer);
                //device.WriteRead(new byte[] { CMD_LED1, 0x00, 0xFF, 0x00 }, i2CReadBuffer);

                //device.WriteRead(new byte[] { 0x14 }, i2CReadBuffer);
                //var a = BitConverter.ToInt16(i2CReadBuffer, 0);

                //device.Write(new byte[] { CMD_MODUS, (byte)Modus.ON });
                //device.WriteRead(new byte[] { 0x01 }, i2CReadBuffer);
                //device.WriteRead(new byte[] { 0x01, 0x01 }, i2CReadBuffer);

                //device.Write(new byte[] { 0x20, 0xFF });
                device.WriteRead(new byte[] { 0x13 }, i2CReadBuffer);
                device.Write(new byte[] { 0x12, 0xFF });

                device.Dispose();


                //using (device = await I2cDevice.FromIdAsync(id, i2cSettings))
                //{
                //    i2CReadBuffer = new byte[4];
                //    device.WriteRead(new byte[] { 0x11 }, i2CReadBuffer);
                //    var b = BitConverter.ToSingle(i2CReadBuffer, 0);
                //}
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: {0}", e.Message);
                return;
            }
        }

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            svwMain.IsPaneOpen = !svwMain.IsPaneOpen;
        }

        private void Button_Click_1(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _tvled.Toggle();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

    
}
