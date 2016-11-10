
namespace FountainJob
{
    enum Modus
    {
        OFF  = 0x00,
        PUMP = 0x10,
        LED  = 0x20,
        ON   = PUMP | LED
    };

    static class Constants
    {
        public const string Openhab_Host    = "192.168.178.69";

        public const string Item_State      = "Marco_Fountain_State";
        public const string Item_Sequence   = "Marco_Fountain_Sequence";
        public const string Item_Bridness   = "Marco_Fountain_Bridness";
        public const string Item_Color      = "Marco_Fountain_Color";
        public const string Item_Waterlevel = "Marco_Fountain_Waterlevel";

        public static byte SLAVE_ADDRESS    = 0x33;

        public static byte CMD_MODUS        = 0x01;
        public static byte CMD_SEQUENCE     = 0x02;
        public static byte CMD_WATERLEVEL   = 0x03;
        public static byte CMD_LED1         = 0x11;
        public static byte CMD_LED2         = 0x12;
        public static byte CMD_LED3         = 0x13;
        public static byte CMD_BRIDNESS     = 0x20;

        public static byte TRUE             = 0x01;
        public static byte FALSE            = 0x00;
    }
}
