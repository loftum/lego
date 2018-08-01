using Devices.Adafruit.LSM9DS1.Accelerometer;
using Unosquare.PiGpio.ManagedModel;

namespace Devices.Adafruit.LSM9DS1
{
    public class TempSettings
    {
        public bool Enabled { get; set; } = true;
    }

    public class Thermometer
    {
        // Temperature: LSB per degree celsius
        public const int LSM9DS1_TEMP_LSB_DEGREE_CELSIUS = 8;  // 1°C = 8, 25° = 200, etc.

        public TempSettings Settings { get; } = new TempSettings();
        public I2cDevice Device { get; }

        public Thermometer(I2cDevice device)
        {
            Device = device;
        }

        public double Read()
        {
            return Device.ReadBlock(AccelRegisters.TEMP_OUT_L, 2).ToUshort() / LSM9DS1_TEMP_LSB_DEGREE_CELSIUS;
        }
    }
}