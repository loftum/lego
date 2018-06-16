using Unosquare.PiGpio.ManagedModel;

namespace Devices.Adafruit.LSM9DS1
{
    public enum AccelSampleRate
    {
        /// <summary>
        /// 10 Hz
        /// </summary>
        _10Hz,
        /// <summary>
        /// 50 Hz
        /// </summary>
        _50Hz,
        /// <summary>
        /// 119 Hz
        /// </summary>
        _119Hz,
        /// <summary>
        /// 238 Hz
        /// </summary>
        _238Hz,
        /// <summary>
        /// 476 Hz
        /// </summary>
        _476Hz,
        /// <summary>
        /// 952 Hz
        /// </summary>
        _952Hz
    }

    public enum AccelBandwidth
    {
        /// <summary>
        /// Determined by sample rate
        /// </summary>
        BySampleRate = -1,
        /// <summary>
        /// 408 Hz
        /// </summary>
        _408Hz = 0,
        /// <summary>
        /// 211 Hz
        /// </summary>
        _211Hz = 1,
        /// <summary>
        /// 105 Hz
        /// </summary>
        _105Hz = 2,
        /// <summary>
        /// 50 Hz
        /// </summary>
        _50Hz = 3
    }

    public enum AccelHighResBandwidth
    {
        /// <summary>
        /// ODR / 50
        /// </summary>
        ODR_50 = 0,
        /// <summary>
        /// ODR / 100
        /// </summary>
        ODR_100 = 1,
        /// <summary>
        /// ODR / 9
        /// </summary>
        ODR_9 = 2,
        /// <summary>
        /// ODR / 400
        /// </summary>
        ODR_400 = 3
    }

    public class AccelSettings
    {
        public bool Enabled { get; set; } = true;
        public bool EnableX { get; set; } = true;
        public bool EnableY { get; set; } = true;
        public bool EnableZ { get; set; } = true;
        public AccelRange Range { get; set; } = AccelRange._2G;
        public AccelSampleRate SampleRate { get; set; } = AccelSampleRate._952Hz;
        public AccelBandwidth Bandwidth { get; set; } = AccelBandwidth.BySampleRate;
        public bool HighResEnable { get; set; }
        public AccelHighResBandwidth HighResBandwidth { get; set; } = AccelHighResBandwidth.ODR_50;
    }

    public class Accel
    {
        public AccelSettings Settings { get; }
        public I2cDevice Device { get; }

        public Accel(I2cDevice device)
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
            Device.Write(AccelRegisters.CTRL_REG5_XL, (byte)regValue);

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
            Device.Write(AccelRegisters.CTRL_REG6_XL, (byte)regValue);

            regValue = 0;
            if (Settings.HighResEnable)
            {
                regValue |= 1 << 7;
                regValue |= ((int) Settings.HighResBandwidth & 0x03) << 5;
            }
            Device.Write(AccelRegisters.CTRL_REG7_XL, (byte) regValue);
        }

        private double GetMgPerLsb()
        {
            switch (Settings.Range)
            {
                case AccelRange._16G: return 0.000_732;
                case AccelRange._8G: return 0.000_244;
                case AccelRange._4G: return 0.000_122;
                case AccelRange._2G: return 0.000_061;
                default: return 0.000_061;
            }
        }

        public Vector3 Read()
        {
            var value = Device.ReadBlock(0x80 | AccelRegisters.OUT_X_L_XL, 6).ToVector3();
            return value * GetMgPerLsb() * Constants.SENSORS_GRAVITY_STANDARD;
        }
    }
}