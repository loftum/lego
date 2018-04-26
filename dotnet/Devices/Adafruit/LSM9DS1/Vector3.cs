namespace Devices.Adafruit.LSM9DS1
{
    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static Vector3 operator *(Vector3 vector, float f)
        {
            return new Vector3(vector.X * f, vector.Y * f, vector.Z * f);
        }

        public static Vector3 operator /(Vector3 vector, float f)
        {
            return new Vector3(vector.X / f, vector.Y / f, vector.Z / f);
        }

        public override string ToString()
        {
            return $"[{X}, {Y}, {Z}]";
        }
    }
}