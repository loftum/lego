using System;
using System.Runtime.InteropServices;

namespace Maths
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Float4
    {
        public static Float4 UnitX = new Float4(1f, 0f, 0f, 0f);
        public static Float4 UnitY = new Float4(0f, 1f, 0f, 0f);
        public static Float4 UnitZ = new Float4(0f, 0f, 1f, 0f);
        public static Float4 UnitW = new Float4(0f, 0f, 0f, 1f);
        public static Float4 One = new Float4(1f, 1f, 1f, 1f);
        public static Float4 Zero = new Float4(0f, 0f, 0f, 0f);
        public static readonly int SizeInBytes = Marshal.SizeOf<Float4>();
        
        public float X;
        public float Y;
        public float Z;
        public float W;

        public Float4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }
        
        public static Float4 operator +(Float4 left, Float4 right)
        {
            left.X += right.X;
            left.Y += right.Y;
            left.Z += right.Z;
            left.W += right.W;
            return left;
        }

        public static Float4 operator -(Float4 left, Float4 right)
        {
            left.X -= right.X;
            left.Y -= right.Y;
            left.Z -= right.Z;
            left.W -= right.W;
            return left;
        }

        public static Float4 operator -(Float4 vec)
        {
            vec.X = -vec.X;
            vec.Y = -vec.Y;
            vec.Z = -vec.Z;
            vec.W = -vec.W;
            return vec;
        }

        public static Float4 operator *(Float4 vec, float scale)
        {
            vec.X *= scale;
            vec.Y *= scale;
            vec.Z *= scale;
            vec.W *= scale;
            return vec;
        }

        public static Float4 operator *(float scale, Float4 vec)
        {
            vec.X *= scale;
            vec.Y *= scale;
            vec.Z *= scale;
            vec.W *= scale;
            return vec;
        }

        public static Float4 operator /(Float4 vec, float scale)
        {
            var num = 1f / scale;
            vec.X *= num;
            vec.Y *= num;
            vec.Z *= num;
            vec.W *= num;
            return vec;
        }

        public static bool operator ==(Float4 left, Float4 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Float4 left, Float4 right)
        {
            return !left.Equals(right);
        }

        public static unsafe explicit operator float*(Float4 v)
        {
            return &v.X;
        }

        public static unsafe explicit operator IntPtr(Float4 v)
        {
            return (IntPtr) (void*) &v.X;
        }

        public bool Equals(Float4 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);
        }

        public override bool Equals(object obj)
        {
            return obj is Float4 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode();
        }

        public override string ToString()
        {
            return $"[{X},{Y},{Z},{W}]";
        }
    }
}