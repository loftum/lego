namespace Devices.Adafruit.LSM9DS1
{
    public enum MagGain
    {
        /// <summary>
        /// +/- 4 gauss
        /// </summary>
        _4GAUSS,
        /// <summary>
        /// +/- 8 gauss
        /// </summary>
        _8GAUSS,
        /// <summary>
        /// +/- 12 gauss
        /// </summary>
        _12GAUSS,
        /// <summary>
        /// +/- 16 gauss
        /// </summary>
        _16GAUSS
    }

    //public struct MagGain
    //{
    //    public int RegMask { get; }
    //    /// <summary>
    //    /// Magnetic Field Strength: gauss range
    //    /// </summary>
    //    public float Gain { get; }

    //    private MagGain(int regMask, float gain)
    //    {
    //        RegMask = regMask;
    //        Gain = gain;
    //    }

    //    /// <summary>
    //    /// +/- 4 gauss
    //    /// </summary>
    //    public static readonly MagGain _4GAUSS = new MagGain(0b00 << 5, 0.000_14F);
    //    /// <summary>
    //    /// +/- 8 gauss
    //    /// </summary>
    //    public static readonly MagGain _8GAUSS = new MagGain(0b01 << 5, 0.000_29F);
    //    /// <summary>
    //    /// +/- 12 gauss
    //    /// </summary>
    //    public static readonly MagGain _12GAUSS = new MagGain(0b10 << 5, 0.000_43F);
    //    /// <summary>
    //    /// +/- 16 gauss
    //    /// </summary>
    //    public static readonly MagGain _16GAUSS = new MagGain(0b11 << 5, 0.000_58F);
    //}
}