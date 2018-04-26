namespace Devices.Adafruit.LSM9DS1
{
    public static class ByteExtensions
    {
        public static Vector3 ToVector3(this byte[] buffer)
        {
            var xlo = buffer[0];
            ushort xhi = buffer[1];
            var ylo = buffer[2];
            ushort yhi = buffer[3];
            var zlo = buffer[4];
            ushort zhi = buffer[5];

            // Shift values to create properly formed integer (low byte first)
            xhi <<= 8; xhi |= xlo;
            yhi <<= 8; yhi |= ylo;
            zhi <<= 8; zhi |= zlo;
            return new Vector3(xhi, yhi, zhi);
        }

        public static ushort ToUshort(this byte[] buffer)
        {
            var lo = buffer[0];
            ushort hi = buffer[1];
            hi <<= 8; hi |= lo;
            return hi;
        }
    }
}