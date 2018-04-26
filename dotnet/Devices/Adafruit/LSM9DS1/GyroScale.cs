namespace Devices.Adafruit.LSM9DS1
{
    public struct GyroScale
    {
        public int RegMask { get; }
        public float DpsPerLsb { get; }

        private GyroScale(int regMask, float dpsPerLsb)
        {
            RegMask = regMask;
            DpsPerLsb = dpsPerLsb;
        }

        /// <summary>
        /// +/- 245 degrees per second rotation 
        /// </summary>
        public static readonly GyroScale _245DPS = new GyroScale(0b00 << 3, 0.00875F);

        /// <summary>
        /// +/- 500 degrees per second rotation
        /// </summary>
        public static readonly GyroScale _500DPS = new GyroScale(0b01 << 3, 0.01750F);

        /// <summary>
        /// +/- 2000 degrees per second rotation
        /// </summary>
        public static readonly GyroScale _2000DPS = new GyroScale(0b11 << 3, 0.07000F);
    }
}