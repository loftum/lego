using Unosquare.RaspberryIO.Gpio;

namespace Devices.Adafruit.LSM9DS1
{
    public static class I2CDeviceExtensions
    {
        public static byte[] ReadAddressBytes(this I2CDevice device, int address, int length)
        {
            var buffer = new byte[length];
            for (var ii = 0; ii < length; ii++)
            {
                buffer[ii] = device.ReadAddressByte(address);
            }
            return buffer;
        }
    }
}