using Maths;

namespace Devices
{
    public static class ByteExtensions
    {
        public static Vector3 ToVector3(this byte[] buffer)
        {
            //var x = (short) (buffer[0] & 0x7fff);
            var x = (short) ((buffer[1] << 8 | buffer[0]) & 0x7fff);
            var y = (short) (buffer[3] << 8 | buffer[2]);
            var z = (short) (buffer[5] << 8 | buffer[4]);
            return new Vector3(x, y, z);
        }

        public static Quaternion ToQuaternion(this byte[] buffer)
        {
            
            var w = (short)(buffer[1] << 8 | buffer[0]);
            var x = (short)(buffer[3] << 8 | buffer[2]);
            var y = (short)(buffer[5] << 8 | buffer[4]);
            var z = (short)(buffer[7] << 8 | buffer[6]);
            return new Quaternion(w, x, y, z);
        }

        public static double ToDouble(this byte[] buffer)
        {
            return buffer[1] << 8 | buffer[0];
        }

        public static int ToUshort(this byte[] buffer)
        {
            return buffer[1] << 8 | buffer[0];
        }

        public static byte Reverse(this byte b)
        {
            byte res = 0;
            for (var ii = 0; ii < 8; ii++)
            {
                var mask = (byte) (1 << (7 - 2 * ii));
                res |= mask;
            }

            return res;
        }
    }
}