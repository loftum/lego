using System;

namespace Maths
{
    public static class MathExtensions
    {
        public static int ToDeg(this double rad)
        {
            return (int)(rad * 180 / Math.PI);
        }
        
        public static Quatf ToQuatF(this Quatd quatd)
        {
            return new Quatf((float) quatd.W, (float) quatd.X, (float) quatd.Y, (float) quatd.Z);
        }
        
        public static Float3 ToFloat3(this Double3 double3)
        {
            return new Float3((float) double3.X, (float) double3.Y, (float) double3.Z);
        }
    }
}

