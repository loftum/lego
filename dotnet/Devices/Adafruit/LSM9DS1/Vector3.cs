namespace Devices.Adafruit.LSM9DS1
{
    public struct Vector3
    {
        public double X;
        public double Y;
        public double Z;

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 operator *(Vector3 vector, double f)
        {
            return new Vector3(vector.X * f, vector.Y * f, vector.Z * f);
        }

        public static Vector3 operator /(Vector3 vector, double f)
        {
            return new Vector3(vector.X / f, vector.Y / f, vector.Z / f);
        }

        public override string ToString()
        {
            return $"[{X}, {Y}, {Z}]";
        }
    }
}