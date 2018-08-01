using Devices.Adafruit.LSM9DS1.Accelerometer;
using Unosquare.PiGpio.ManagedModel;

namespace Devices.Adafruit.LSM9DS1.Gyroscope
{
    public class Gyro
    {
        public GyroSettings Settings { get; } = new GyroSettings();
        public I2cDevice Device { get; }

        public Gyro(I2cDevice device)
        {
            Device = device;
            Init();
        }

        public void Init()
        {
            var regValue = 0;
            if (Settings.Enabled)
            {
                regValue = ((int)Settings.SampleRate & 0x07) << 5;
            }

            switch (Settings.Scale)
            {
                case GyroScale._500DPS:
                    regValue |= 0x1 << 3;
                    break;
                case GyroScale._2000DPS:
                    regValue |= 0x3 << 3;
                    break;
            }
            regValue |= Settings.Bandwidth & 0x03;
            Device.Write(AccelRegisters.CTRL_REG1_G, (byte)regValue);
            Device.Write(AccelRegisters.CTRL_REG2_G, 0x00);

            regValue = Settings.LowPowerEnable ? 1 << 7 : 0;
            if (Settings.HPFEnable)
            {
                regValue |= 1 << 6 | (Settings.HPFCutoff & 0x0f);
            }
            Device.Write(AccelRegisters.CTRL_REG3_G, (byte)regValue);

            regValue = 0;
            if (Settings.EnableZ)
            {
                regValue |= 1 << 5;
            }

            if (Settings.EnableY)
            {
                regValue |= 1 << 4;
            }

            if (Settings.EnableX)
            {
                regValue |= 1 << 3;
            }

            if (Settings.LatchInterrupt)
            {
                regValue |= 1 << 1;
            }
            Device.Write(AccelRegisters.CTRL_REG4, (byte)regValue);

            regValue = 0;
            if (Settings.FlipX)
            {
                regValue |= 1 << 5;
            }

            if (Settings.FlipY)
            {
                regValue |= 1 << 4;
            }

            if (Settings.FlipZ)
            {
                regValue |= 1 << 3;
            }
            Device.Write(AccelRegisters.ORIENT_CFG_G, (byte) regValue);
        }

        public Vector3 Read()
        {
            return Device.ReadBlock(0x80 | AccelRegisters.OUT_X_L_G, 6).ToVector3() * Settings.GetScale();
        }
    }
}