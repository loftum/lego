namespace Devices.Adafruit
{
    public static class Constants
    {
        public const float SENSORS_GRAVITY_EARTH = 9.80665F;             /**< Earth's gravity in m/s^2 */
        public const float SENSORS_GRAVITY_MOON = 1.6F;                  /**< The moon's gravity in m/s^2 */
        public const float SENSORS_GRAVITY_SUN = 275.0F;                /**< The sun's gravity in m/s^2 */
        public const float SENSORS_GRAVITY_STANDARD = SENSORS_GRAVITY_EARTH;
        public const float SENSORS_MAGFIELD_EARTH_MAX = 60.0F;                 /**< Maximum magnetic field on Earth's surface */
        public const float SENSORS_MAGFIELD_EARTH_MIN = 30.0F;                 /**< Minimum magnetic field on Earth's surface */
        public const float SENSORS_PRESSURE_SEALEVELHPA = 1013.25F;              /**< Average sea level pressure is 1013.25 hPa */
        public const float SENSORS_DPS_TO_RADS = 0.017453293F;          /**< Degrees/s to rad/s multiplier */
        public const float SENSORS_GAUSS_TO_MICROTESLA = 100; /**< Gauss to micro-Tesla multiplier */

    }
}