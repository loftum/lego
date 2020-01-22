namespace Maths
{
    public struct Double3
    {
        public double X;
        public double Y;
        public double Z;

        public Double3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Double3 operator +(Double3 first, Double3 second)
        {
            return new Double3(first.X + second.X, first.Y + second.Y, first.Z + second.Z);
        }

        public static Double3 operator *(Double3 vector, double f)
        {
            return new Double3(vector.X * f, vector.Y * f, vector.Z * f);
        }

        public static Double3 operator *(double f, Double3 vector)
        {
            return vector * f;
        }

        public static Double3 operator /(Double3 vector, double f)
        {
            return new Double3(vector.X / f, vector.Y / f, vector.Z / f);
        }

        public static implicit operator Double3(double value)
        {
            return new Double3(value, value, value);
        }

        public static Double3 Zero => new Double3();

        public override string ToString()
        {
            return ToString(null);
        }

        public string ToString(string format)
        {
            return $"[{X.ToString(format)},{Y.ToString(format)},{Z.ToString(format)}]";
        }

        public static bool TryParse(string stringValue, out Double3 result)
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

            if (double.TryParse(parts[0], out var x) &&
                double.TryParse(parts[1], out var y) &&
                double.TryParse(parts[2], out var z))
            {
                result = new Double3(x, y, z);
                return true;
            }
            return false;
        }
    }
}