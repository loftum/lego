namespace Devices.Adafruit.LSM9DS1
{
    public static class AccelRegisters
    {
        public const int ORIENT_CFG_G = 0x13;
        public const int WHO_AM_I_XG = 0x0F;
        public const int CTRL_REG1_G = 0x10;
        public const int CTRL_REG2_G = 0x11;
        public const int CTRL_REG3_G = 0x12;
        public const int TEMP_OUT_L = 0x15;
        public const int TEMP_OUT_H = 0x16;
        public const int STATUS_REG = 0x17;
        public const byte OUT_X_L_G = 0x18;
        public const int OUT_X_H_G = 0x19;
        public const int OUT_Y_L_G = 0x1A;
        public const int OUT_Y_H_G = 0x1B;
        public const int OUT_Z_L_G = 0x1C;
        public const int OUT_Z_H_G = 0x1D;
        public const int CTRL_REG4 = 0x1E;
        public const int CTRL_REG5_XL = 0x1F;
        public const int CTRL_REG6_XL = 0x20;
        public const int CTRL_REG7_XL = 0x21;
        public const int CTRL_REG8 = 0x22;
        public const int CTRL_REG9 = 0x23;
        public const int CTRL_REG10 = 0x24;
        public const byte OUT_X_L_XL = 0x28;
        public const int OUT_X_H_XL = 0x29;
        public const int OUT_Y_L_XL = 0x2A;
        public const int OUT_Y_H_XL = 0x2B;
        public const int OUT_Z_L_XL = 0x2C;
        public const int OUT_Z_H_XL = 0x2D;
    }
}