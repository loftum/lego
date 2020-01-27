﻿namespace Devices.Distance.Sharp.GP2Y0A41SK0F
{
    public struct PlotSample
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
            var y = previous.Y + x * dy / dx;
            return y;
        }
    }
}