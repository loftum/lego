using System;

namespace Maths
{
    public static class MathExtensions
    {
        public static int ToDeg(this double rad)
        {
            return (int)(rad * 180 / Math.PI);
        }
    }
}

