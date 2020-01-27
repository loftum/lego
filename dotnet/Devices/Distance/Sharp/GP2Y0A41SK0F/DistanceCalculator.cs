using System;
using System.Collections.Generic;
using System.Linq;

namespace Devices.Distance.Sharp.GP2Y0A41SK0F
{
    public class DistanceCalculator
    {
        private readonly List<PlotSample> _samples;

        public DistanceCalculator(IEnumerable<PlotSample> samples)
        {
            _samples = samples.OrderByDescending(s => s.X).ToList();
        }

        public double CalculateDistance(double voltage)
        {
            var previous = new PlotSample(double.MaxValue, 0);
            
            foreach (var next in _samples)
            {
                if (next.X > voltage)
                {
                    previous = next;
                    continue;
                }

                var cm = PlotSample.CalculateLinear(previous, next, voltage);
                Console.WriteLine($"V:{voltage}, cm:{cm}");
                return cm;
            }
            return double.MaxValue;
        }
    }
}