namespace Devices.Adafruit.LSM9DS1.Accelerometer
{
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

        public double GetMgPerLsb()
        {
            switch (Range)
            {
                case AccelRange._16G: return 0.000_732;
                case AccelRange._8G: return 0.000_244;
                case AccelRange._4G: return 0.000_122;
                case AccelRange._2G: return 0.000_061;
                default: return 0.000_061;
            }
        }
    }
}