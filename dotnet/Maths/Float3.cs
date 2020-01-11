using System;
using System.Runtime.InteropServices;

namespace Maths
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Float3
    {
        public static Float3 Zero => new Float3();
        public static Float3 One => new Float3(1f, 1f, 1f);
        
        public float X;
        public float Y;
        public float Z;

        public float Length => (float) Math.Sqrt(X * X + Y * Y + Z * Z);

        public Float3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Float3 operator +(Float3 first, Float3 second)
        {
            return new Float3(first.X + second.X, first.Y + second.Y, first.Z + second.Z);
        }
        
        public static Float3 operator -(Float3 first)
        {
            return new Float3(-first.X, -first.Y, -first.Z);
        }

        public static Float3 operator *(Float3 vector, float f)
        {
            return new Float3(vector.X * f, vector.Y * f, vector.Z * f);
        }

        public static Float3 operator *(float f, Float3 vector)
        {
            return vector * f;
        }

        public static Float3 operator /(Float3 vector, float f)
        {
            return new Float3(vector.X / f, vector.Y / f, vector.Z / f);
        }

        public static implicit operator Float3(float value)
        {
            return new Float3(value, value, value);
        }

        public override string ToString()
        {
            return ToString(null);
        }

        public string ToString(string format)
        {
            return $"[{X.ToString(format)},{Y.ToString(format)},{Z.ToString(format)}]";
        }

        public void Normalize()
        {
            var num = 1f / Length;
            X *= num;
            Y *= num;
            Z *= num;
        }
        
        public static bool TryParse(string stringValue, out Float3 result)
        {
            result = Zero;
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return false;
            }

            var parts = stringValue.TrimStart('[').TrimEnd(']').Split(',');
            if (parts.Length != 3)
            {
                return false;
            }

            if (float.TryParse(parts[0], out var x) &&
                float.TryParse(parts[1], out var y) &&
                float.TryParse(parts[2], out var z))
            {
                result = new Float3(x, y, z);
                return true;
            }
            return false;
        }
    }
}