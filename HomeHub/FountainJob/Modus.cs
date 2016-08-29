
namespace FountainJob
{
    enum Modus
    {
        OFF     = 0x00,
        PUMP    = 0x10,
        LED     = 0x20,
        ON      = PUMP | LED
    };
}
