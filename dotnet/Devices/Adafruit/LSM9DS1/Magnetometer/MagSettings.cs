namespace Devices.Adafruit.LSM9DS1.Magnetometer
{
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

        public double GetGain()
        {
            switch (MagGain)
            {
                case MagGain._16GAUSS: return 0.000_58;
                case MagGain._12GAUSS: return 0.000_43;
                case MagGain._8GAUSS: return 0.000_29;
                case MagGain._4GAUSS: return 0.000_14;
                default: return 0.000_14;
            }
        }
    }
}