namespace Devices.ABElectronics.ServoPWMPiZero
{
    public enum Registers
    {
        MODE1 = 0x00,
        MODE2 = 0x01,
        SUABDR1 = 0x02,
        SUBADR2 = 0x03,
        ALLCALLADR = 0x05,
        LED0_ON_L = 0x06,
        LED0_ON_H = 0x07,
        LED0_OFF_L = 0x08,
        LED0_OFF_H = 0x09,

        ALL_LED_ON_L = 0xFA,
        ALL_LED_ON_H = 0xFB,
        ALL_LED_OFF_L = 0xFC,
        ALL_LED_OFF_H = 0xFD,
        PRE_SCALE = 0xFE,
        TestMode = 0xFF
    }
}