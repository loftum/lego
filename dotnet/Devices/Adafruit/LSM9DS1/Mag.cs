using Unosquare.RaspberryIO.Gpio;

namespace Devices.Adafruit.LSM9DS1
{
    public enum MagSampleRate
    {
        /// <summary>
        /// 0.625 Hz
        /// </summary>
        _0_625Hz,
        /// <summary>
        /// 1.25 Hz
        /// </summary>
        _1_25Hz,
        /// <summary>
        /// 2.5 Hz
        /// </summary>
        _2_5Hz,
        /// <summary>
        /// 5 Hz
        /// </summary>
        _5Hz,
        /// <summary>
        /// 10 Hz
        /// </summary>
        _10Hz,
        /// <summary>
        /// 20 Hz
        /// </summary>
        _20Hz,
        /// <summary>
        /// 40 Hz
        /// </summary>
        _40Hz,
        /// <summary>
        /// 80 Hz
        /// </summary>
        _80Hz
    }

    public enum MagPerformance
    {
        LowPowerMode = 0,
        MediumPerformance = 1,
        HighPeroformance = 2,
        UltraHighPerformance = 3
    }

    public enum MagOperatingMode
    {
        ContinuousConversion,
        SingleConversion,
        PowerDown
    }

    public class MagSettings
    {
        public bool Enabled { get; set; } = true;
        public MagGain MagGain { get; set; } = MagGain._4GAUSS;
        public MagSampleRate SampleRate { get; set; } = MagSampleRate._80Hz;
        public bool TempCompensationEnable { get; set; }
        public MagPerformance XYPerformance { get; set; } = MagPerformance.UltraHighPerformance;
        public MagPerformance ZPerformance { get; set; } = MagPerformance.UltraHighPerformance;
        public bool LowPowerEnable { get; set; }
        public MagOperatingMode OperatingMode { get; set; }
    }

    public class Mag
    {
        public MagSettings Settings { get; } = new MagSettings();
        private readonly I2CDevice _device;

        public Mag(I2CDevice device)
        {
            _device = device;
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
            _device.WriteAddressByte(MagRegisters.CTRL_REG1_M, (byte) regValue);

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
            _device.WriteAddressByte(MagRegisters.CTRL_REG2_M, (byte) regValue);

            regValue = 0;
            if (Settings.LowPowerEnable)
            {
                regValue |= 1 << 5;
            }

            regValue |= (int) Settings.OperatingMode & 0x03;
            _device.WriteAddressByte(MagRegisters.CTRL_REG3_M, (byte) regValue);

            regValue = 0;
            regValue = ((int) Settings.ZPerformance & 0x03) << 2;
            _device.WriteAddressByte(MagRegisters.CTRL_REG4_M, (byte) regValue);

            regValue = 0;
            _device.WriteAddressByte(MagRegisters.CTRL_REG5_M, (byte) regValue);
        }

        private double GetGain()
        {
            switch (Settings.MagGain)
            {
                case MagGain._16GAUSS: return 0.000_58;
                case MagGain._12GAUSS: return 0.000_43;
                case MagGain._8GAUSS: return 0.000_29;
                case MagGain._4GAUSS: return 0.000_14;
                default: return 0.000_14;
            }
        }

        public Vector3 Read()
        {
            return _device.ReadAddressBytes(0x80 | MagRegisters.OUT_X_L_M, 6).ToVector3() * GetGain();
        }
    }
}