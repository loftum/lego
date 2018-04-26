namespace Devices.Adafruit.LSM9DS1
{
    public struct MagGain
    {
        public int RegMask { get; }
        /// <summary>
        /// Magnetic Field Strength: gauss range
        /// </summary>
        public float Gain { get; }

        private MagGain(int regMask, float gain)
        {
            RegMask = regMask;
            Gain = gain;
        }

        /// <summary>
        /// +/- 4 gauss
        /// </summary>
        public static readonly MagGain _4GAUSS = new MagGain(0b00 << 5, 0.14F);
        /// <summary>
        /// +/- 8 gauss
        /// </summary>
        public static readonly MagGain _8GAUSS = new MagGain(0b01 << 5, 0.29F);
        /// <summary>
        /// +/- 12 gauss
        /// </summary>
        public static readonly MagGain _12GAUSS = new MagGain(0b10 << 5, 0.43F);
        /// <summary>
        /// +/- 16 gauss
        /// </summary>
        public static readonly MagGain _16GAUSS = new MagGain(0b11 << 5, 0.58F);
    }
}