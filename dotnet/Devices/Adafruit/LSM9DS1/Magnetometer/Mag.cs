using Devices.Unosquare;
using Unosquare.RaspberryIO.Abstractions;

namespace Devices.Adafruit.LSM9DS1.Magnetometer
{
    public class Mag
    {
        public MagSettings Settings { get; } = new MagSettings();
        public II2CDevice Device { get; }

        public Mag(II2CDevice device)
        {
            Device = device;
            Init();
        }

        public void Init()
        {
            var regValue = 0;

            if (Settings.TempCompensationEnable)
            {
                regValue |= 1 << 7;
            }

            regValue |= ((int) Settings.XYPerformance % 0x03) << 5;
            regValue |= ((int) Settings.SampleRate & 0x07) << 2;
            Device.WriteAddressByte(MagRegisters.CTRL_REG1_M, (byte) regValue);

            regValue = 0;
            switch (Settings.MagGain)
            {
                case MagGain._8GAUSS:
                    regValue |= 0x1 << 5;
                    break;
                case MagGain._12GAUSS:
                    regValue |= 0x2 << 5;
                    break;
                case MagGain._16GAUSS:
                    regValue |= 0X3 << 5;
                    break;
            }
            Device.WriteAddressByte(MagRegisters.CTRL_REG2_M, (byte) regValue);

            regValue = 0;
            if (Settings.LowPowerEnable)
            {
                regValue |= 1 << 5;
            }

            regValue |= (int) Settings.OperatingMode & 0x03;
            Device.WriteAddressByte(MagRegisters.CTRL_REG3_M, (byte) regValue);

            regValue = 0;
            regValue = ((int) Settings.ZPerformance & 0x03) << 2;
            Device.WriteAddressByte(MagRegisters.CTRL_REG4_M, (byte) regValue);

            regValue = 0;
            Device.WriteAddressByte(MagRegisters.CTRL_REG5_M, (byte) regValue);
        }

        public Vector3 Read()
        {
            return Device.ReadBlock(0x80 | MagRegisters.OUT_X_L_M, 6).ToVector3() * Settings.GetGain();
        }
    }
}