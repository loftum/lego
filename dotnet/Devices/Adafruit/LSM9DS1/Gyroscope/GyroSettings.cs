namespace Devices.Adafruit.LSM9DS1.Gyroscope
{
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

        public double GetScale()
        {
            switch (Scale)
            {
                case GyroScale._2000DPS: return 0.07000;
                case GyroScale._500DPS: return 0.01750;
                case GyroScale._245DPS: return 0.00875;
                default: return 0.00875;
            }
        }
    }
}