namespace Devices
{
    public struct Quaternion
    {
        public double W { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Quaternion(double w, double x, double y, double z)
        {
            W = w;
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return $"[{W}, {X}, {Y}, {Z}]";
        }
    }
}