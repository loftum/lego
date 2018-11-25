namespace Devices.Adafruit.BNO055
{
    public enum FusionDataFormat
    {
        /// <summary>
        /// Roll: -90° to +90°
        /// Pitch: -180° to +180°
        /// Yaw: 0° to 360°
        /// </summary>
        Windows = 0,
        /// <summary>
        /// Roll: -90° to +90°
        /// Pitch: +180° to -180°
        /// Yaw: 0° to 360°
        /// </summary>
        Android = 1
    }
}