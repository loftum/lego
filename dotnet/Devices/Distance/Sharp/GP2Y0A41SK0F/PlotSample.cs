namespace Devices.Distance.Sharp.GP2Y0A41SK0F
{
    public readonly struct PlotSample
    {
        public double X { get; }
        public double Y { get; }

        public PlotSample(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static double CalculateLinear(PlotSample previous, PlotSample next, double x)
        {
            var dy = next.Y - previous.Y;
            var dx = next.X - previous.X;
            var deltaX = x - previous.X;
            var deltaY = deltaX * dy / dx;
            return previous.Y + deltaY;
        }
    }
}