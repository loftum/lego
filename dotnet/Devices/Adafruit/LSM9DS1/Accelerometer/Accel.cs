using Devices.Unosquare;
using Unosquare.RaspberryIO.Abstractions;

namespace Devices.Adafruit.LSM9DS1.Accelerometer
{
    public class Accel
    {
        public AccelSettings Settings { get; }
        public II2CDevice Device { get; }

        public Accel(II2CDevice device)
        {
            Device = device;
            Settings = new AccelSettings();
            Init();
        }

        private void Init()
        {
            var regValue = 0;
            if (Settings.EnableX)
            {
                regValue |= 1 << 5;
            }

            if (Settings.EnableY)
            {
                regValue |= 1 << 4;
            }

            if (Settings.EnableZ)
            {
                regValue |= 1 << 3;
            }
            Device.WriteAddressByte(AccelRegisters.CTRL_REG5_XL, (byte)regValue);

            regValue = 0;
            if (Settings.Enabled)
            {
                switch (Settings.Range)
                {
                    case AccelRange._4G:
                        regValue |= 0x2 << 3;
                        break;
                    case AccelRange._8G:
                        regValue |= 0x3 << 3;
                        break;
                    case AccelRange._16G:
                        regValue |= 0x1 << 3;
                        break;
                }
            }

            switch (Settings.Bandwidth)
            {
                case AccelBandwidth.BySampleRate:
                    break;
                default:
                    regValue |= 1 << 2; // Set BW_SCAL_ODR
                    regValue |= (int) Settings.Bandwidth & 0x03;
                    break;
            }
            Device.WriteAddressByte(AccelRegisters.CTRL_REG6_XL, (byte)regValue);

            regValue = 0;
            if (Settings.HighResEnable)
            {
                regValue |= 1 << 7;
                regValue |= ((int) Settings.HighResBandwidth & 0x03) << 5;
            }
            Device.WriteAddressByte(AccelRegisters.CTRL_REG7_XL, (byte) regValue);
        }

        public Vector3 Read()
        {
            var value = Device.ReadBlock(0x80 | AccelRegisters.OUT_X_L_XL, 6).ToVector3();
            return value * Settings.GetMgPerLsb() * Constants.SENSORS_GRAVITY_STANDARD;
        }
    }
}