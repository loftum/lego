namespace Devices.Adafruit.LSM9DS1
{
    public static class MagRegisters
    {
        public const int WHO_AM_I_M = 0x0F;
        public const int CTRL_REG1_M = 0x20;
        public const int CTRL_REG2_M = 0x21;
        public const int CTRL_REG3_M = 0x22;
        public const int CTRL_REG4_M = 0x23;
        public const int CTRL_REG5_M = 0x24;
        public const int STATUS_REG_M = 0x27;
        public const int OUT_X_L_M = 0x28;
        public const int OUT_X_H_M = 0x29;
        public const int OUT_Y_L_M = 0x2A;
        public const int OUT_Y_H_M = 0x2B;
        public const int OUT_Z_L_M = 0x2C;
        public const int OUT_Z_H_M = 0x2D;
        public const int CFG_M = 0x30;
        public const int INT_SRC_M = 0x31;
    }
}