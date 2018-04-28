using Unosquare.RaspberryIO.Gpio;

namespace Devices.Adafruit.LSM9DS1
{
    public enum GyroSampleRate
    {
        /// <summary>
        /// 14.9
        /// </summary>
        _14_9,
        /// <summary>
        /// 59.5
        /// </summary>
        _59_5,
        /// <summary>
        /// 119
        /// </summary>
        _119,
        /// <summary>
        /// 238
        /// </summary>
        _238,
        /// <summary>
        /// 476
        /// </summary>
        _476,
        /// <summary>
        /// 952
        /// </summary>
        _952,
    }

    public class GyroSettings
    {
        public bool Enabled { get; set; } = true;
        public bool EnableX { get; set; } = true;
        public bool EnableY { get; set; } = true;
        public bool EnableZ { get; set; } = true;
        public GyroScale Scale { get; set; } = GyroScale._245DPS;
        public GyroSampleRate SampleRate { get; set; } = GyroSampleRate._952;
        /// <summary>
        /// 0-3
        /// </summary>
        public int Bandwidth { get; set; }
        public bool LowPowerEnable { get; set; }
        public bool HPFEnable { get; set; }
        public int HPFCutoff { get; set; }
        public bool FlipX { get; set; }
        public bool FlipY { get; set; }
        public bool FlipZ { get; set; }
        public int Orientation { get; set; }
        public bool LatchInterrupt { get; set; } = true;
    }

    public class Gyro
    {
        public GyroSettings Settings { get; } = new GyroSettings();

        private readonly I2CDevice _device;

        public Gyro(I2CDevice device)
        {
            _device = device;
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
            _device.WriteAddressByte(AccelRegisters.CTRL_REG1_G, (byte)regValue);
            _device.WriteAddressByte(AccelRegisters.CTRL_REG2_G, 0x00);

            regValue = Settings.LowPowerEnable ? 1 << 7 : 0;
            if (Settings.HPFEnable)
            {
                regValue |= 1 << 6 | (Settings.HPFCutoff & 0x0f);
            }
            _device.WriteAddressByte(AccelRegisters.CTRL_REG3_G, (byte)regValue);

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
            _device.WriteAddressByte(AccelRegisters.CTRL_REG4, (byte)regValue);

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
            _device.WriteAddressByte(AccelRegisters.ORIENT_CFG_G, (byte) regValue);
        }

        private double GetScale()
        {
            switch (Settings.Scale)
            {
                case GyroScale._2000DPS: return 0.07000;
                case GyroScale._500DPS: return 0.01750;
                case GyroScale._245DPS: return 0.00875;
                default: return 0.00875;
            }
        }

        public Vector3 Read()
        {
            return _device.ReadAddressBytes(0x80 | AccelRegisters.OUT_X_L_G, 6).ToVector3() * GetScale();
        }
    }
}