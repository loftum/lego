namespace Devices.Adafruit.LSM9DS1.Accelerometer
{
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
}