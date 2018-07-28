namespace Devices.Adafruit.LSM9DS1
{
    public static class ByteExtensions
    {
        public static Vector3 ToVector3(this byte[] buffer)
        {
            var x = (short) (buffer[1] << 8 | buffer[0]);
            var y = (short) (buffer[3] << 8 | buffer[2]);
            var z = (short) (buffer[5] << 8 | buffer[4]);
            return new Vector3(x, y, z);
        }

        public static int ToUshort(this byte[] buffer)
        {
            return buffer[1] << 8 | buffer[0];
        }
    }
}