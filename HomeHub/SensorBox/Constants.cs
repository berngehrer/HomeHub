
namespace SensorBox
{
    static class Constants
    {
        public const string Openhab_Host    = "192.168.178.69";

        public const string Item_Temp       = "Marco_Sensor1_Temp";
        public const string Item_Hum        = "Marco_Sensor1_Hum";
        public const string Item_PPM        = "Marco_Sensor1_PPM";
        public const string Item_Lux        = "Marco_Sensor1_Lux";

        public static byte SLAVE_ADDRESS    = 0x34;

        public static byte CMD_TEMP         = 0x01;
        public static byte CMD_HUM          = 0x02;
        public static byte CMD_GAS          = 0x03;
        public static byte CMD_LUX          = 0x04;
    }
}
