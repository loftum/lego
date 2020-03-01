namespace Devices.ThePiHut.ADCPiZero
{
    /// <summary>
    /// Data rate.
    /// The higher data rate, the fewer samples per second
    /// </summary>
    public enum Bitrate
    {
        /// <summary>
        /// 12 bits: 240 samples per second
        /// </summary>
        _12 = 12,
        /// <summary>
        /// 14 bits: 60 samples per second
        /// </summary>
        _14 = 14,
        /// <summary>
        /// 16 bits: 15 samples per second
        /// </summary>
        _16 = 16,
        /// <summary>
        /// 18 bits: 3.75 samples per second
        /// </summary>
        _18 = 18
    }
}
