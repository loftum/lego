using System;

namespace Maths
{
    public struct Quatd
    {
        public static readonly Quatd Identity = new Quatd(0, 0, 0, 1);
        
        public double W { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Quatd(double w, double x, double y, double z)
        {
            W = w;
            X = x;
            Y = y;
            Z = z;
        }

        public Quatd(double rotationX, double rotationY, double rotationZ)
        {
            rotationX *= 0.5;
            rotationY *= 0.5;
            rotationZ *= 0.5;

            var c1 = Math.Cos(rotationX);
            var c2 = Math.Cos(rotationY);
            var c3 = Math.Cos(rotationZ);
            var s1 = Math.Sin(rotationX);
            var s2 = Math.Sin(rotationY);
            var s3 = Math.Sin(rotationZ);

            W = (c1 * c2 * c3) - (s1 * s2 * s3);
            X = (s1 * c2 * c3) + (c1 * s2 * s3);
            Y = (c1 * s2 * c3) - (s1 * c2 * s3);
            Z = (c1 * c2 * s3) + (s1 * s2 * c3);
        }
        
        public static Quatd operator /(Quatd vector, double f)
        {
            return new Quatd(vector.W / f, vector.X / f, vector.Y / f, vector.Z / f);
        }

        public override string ToString()
        {
            return $"[{W},{X},{Y},{Z}]";
        }

        public static bool TryParse(string stringValue, out Quatd result)
        {
            result = Identity;
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return false;
            }

            var parts = stringValue.TrimStart('[').TrimEnd(']').Split(',');
            if (parts.Length != 4)
            {
                return false;
            }

            if (double.TryParse(parts[0], out var w) &&
                double.TryParse(parts[1], out var x) &&
                double.TryParse(parts[2], out var y) &&
                double.TryParse(parts[3], out var z))
            {
                result = new Quatd(w, x, y, z);
                return true;
            }
            return false;
        }
    }
}