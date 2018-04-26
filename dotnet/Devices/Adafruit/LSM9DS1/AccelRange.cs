namespace Devices.Adafruit.LSM9DS1
{
    /// <summary>
    /// Linear acceleration: mg per LSB
    /// </summary>
    public struct AccelRange
    {
        public int RegMask { get; }
        
        /// <summary>
        /// mg per LSB
        /// </summary>
        public float MgPerLsb { get; }

        private AccelRange(int regMask, float mgPerLsb)
        {
            RegMask = regMask;
            MgPerLsb = mgPerLsb;
        }

        public static readonly AccelRange _2G = new AccelRange(0b00 << 3, 0.061F);
        public static readonly AccelRange _4G = new AccelRange(0b10 << 3, 0.122F);
        public static readonly AccelRange _8G = new AccelRange(0b11 << 3, 0.244F);
        public static readonly AccelRange _16G = new AccelRange(0b01 << 3, 0.732F);
    }
}