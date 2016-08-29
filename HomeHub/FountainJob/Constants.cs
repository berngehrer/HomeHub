
using System;

namespace FountainJob
{
    static class Constants
    {
        public static TimeSpan PingDelay = TimeSpan.FromSeconds(30);

        public static string Host = "192.168.178.69";
        public static string Item = "Marco_Room_Brunnen";

        public static byte SLAVE_ADDRESS    = 0x33;

        public static byte CMD_MODUS        = 0x01;
        public static byte CMD_REMOTE       = 0x02;
        public static byte CMD_SEQUENCE     = 0x03;
        public static byte CMD_LED1         = 0x11;
        public static byte CMD_LED2         = 0x12;
        public static byte CMD_LED3         = 0x13;
        public static byte CMD_BRIDNESS     = 0x20;

        //public static byte TRUE     = 0x01;
        //public static byte FALSE    = 0x00;
    }
}
