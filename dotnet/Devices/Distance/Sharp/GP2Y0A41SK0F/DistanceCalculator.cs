using System;
using System.Collections.Generic;
using System.Linq;

namespace Devices.Distance.Sharp.GP2Y0A41SK0F
{
    public class DistanceCalculator
    {
        private readonly double _min;
        private readonly double _max;
        private readonly List<PlotSample> _samples;

        public DistanceCalculator(IEnumerable<PlotSample> samples)
        {
            _samples = samples.OrderBy(s => s.X).ToList();
            _min = samples.Min(s => s.X);
            _max = samples.Max(s => s.X);
        }

        public double CalculateDistance(double voltage)
        {
            if (voltage < _min)
            {
                return double.MaxValue;
            }

            if (voltage > _max)
            {
                return double.MinValue;
            }
            var previous = _samples.First();
            
            foreach (var next in _samples.Skip(1))
            {
                if (next.X < voltage)
                {
                    previous = next;
                    continue;
                }

                var cm = PlotSample.CalculateLinear(previous, next, voltage);
                //Console.WriteLine($"V:{voltage}, cm:{cm}");
                return cm;
            }
            return double.MinValue;
        }
    }
}