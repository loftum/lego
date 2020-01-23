using System;

namespace Maths
{
    public static class Float
    {
        public const float PI = (float) Math.PI;

        public static float Cos(float angle) => (float) Math.Cos(angle);
        public static float Sin(float angle) => (float) Math.Sin(angle);
        public static float Tan(float angle) => (float) Math.Tan(angle);

        public static float Acos(float angle) => (float) Math.Acos(angle);

        public static float Sqrt(double number) => (float) Math.Sqrt(number);
    }
}