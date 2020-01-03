using System;
using System.Collections.Generic;
using System.Text;
using Maths;

namespace Devices
{
    public struct ComposedShort
    {
        public byte Lsb { get; }
        public byte Msb { get; }
        public short ShortValue => (short) (Msb << 8 | Lsb);
        
        public ComposedShort(byte lsb, byte msb)
        {
            Lsb = lsb;
            Msb = msb;
        }

        public override string ToString()
        {
            return ToBinaryString();
        }

        public string ToBinaryString()
        {
            return $"{Msb.ToBinaryString()} {Lsb.ToBinaryString()}";
        }
    }
    
    public static class Bytes
    {
        public static ComposedShort[] ToComposedShorts(this byte[] bytes)
        {
            var ret = new ComposedShort[bytes.Length / 2];
            for (var ii = 0; ii < ret.Length; ii ++)
            {
                ret[ii] = new ComposedShort(bytes[ii * 2], bytes[ii * 2 + 1]);
            }

            return ret;
        }
        
        public static string ToBinaryString(this byte b)
        {
            return ZeroPad(Convert.ToString(b, 2), 8);
        }
        
        private static string ZeroPad(string value, int length)
        {
            if (value.Length >= length)
            {
                return value;
            }
            var builder = new StringBuilder(value);
            while (builder.Length < length)
            {
                builder.Insert(0, "0");
            }

            return builder.ToString();
        }
        
        public static bool TryParse(IList<string> values, out byte[] bytes)
        {
            bytes = new byte[values.Count];
            var ii = 0;
            foreach (var value in values)
            {
                if (!byte.TryParse(value, out var b))
                {
                    return false;
                }
                bytes[ii] = b;
                ii++;
            }

            return true;
        }
        
        public static byte[] FixMsb(this byte[] bytes)
        {
            // Stupid bug in the chip.
            // Sometimes msb is set for no apparent reason, which makes the int16 value negative.
            // Let's MacGyver it:
            for (var ii = 1; ii < 6; ii += 2)
            {
                var msb = bytes[ii];
                if (msb > 0b_0000_1011 && msb <= 0b_1001_0110)
                {
                    bytes[ii] &= 0b0111_1111; // Reset MSB
                }
            }

            return bytes;
        }
        
        public static Vector3 ToVector3(this byte[] buffer)
        {
            var x = (short) (buffer[1] << 8 | buffer[0]);
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