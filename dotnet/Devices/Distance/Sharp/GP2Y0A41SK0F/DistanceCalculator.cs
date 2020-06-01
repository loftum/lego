using System.Collections.Generic;
using System.Linq;

namespace Devices.Distance.Sharp.GP2Y0A41SK0F
{
    public class DistanceCalculator
    {
        private readonly double _minVoltage;
        private readonly double _maxVoltage;
        private readonly List<PlotSample> _samples;

        public DistanceCalculator(IEnumerable<PlotSample> samples)
        {
            _samples = samples.OrderBy(s => s.X).ToList();
            _minVoltage = samples.Min(s => s.X);
            _maxVoltage = samples.Max(s => s.X);
            MinDistance = samples.Min(s => s.Y);
            MaxDistance = samples.Max(s => s.Y);
        }

        public double MinDistance { get; }
        public double MaxDistance { get; }

        public CalculatedValue CalculateDistance(double voltage)
        {
            // High voltage -> short distance
            if (voltage < _minVoltage)
            {
                return new CalculatedValue(MaxDistance, Rangeness.AboveMax);
            }

            // Low voltage -> long distance
            if (voltage > _maxVoltage)
            {
                return new CalculatedValue(MinDistance, Rangeness.BelowMin);
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
                return new CalculatedValue(cm, Rangeness.WithinRange);
            }
            return new CalculatedValue(MinDistance, Rangeness.BelowMin);
        }
    }

    public readonly struct CalculatedValue
    {
        public double Value { get; }
        public Rangeness Rangeness { get; }
        
        public CalculatedValue(double value, Rangeness rangeness)
        {
            Value = value;
            Rangeness = rangeness;
        }

        public override string ToString()
        {
            switch (Rangeness)
            {
                case Rangeness.AboveMax:
                    return $"{Value} (Above max)";
                case Rangeness.BelowMin:
                    return $"{Value} (Below min)";
                default:
                    return $"{Value}";
            }
        }

        public static CalculatedValue AboveMax => new CalculatedValue(double.MaxValue, Rangeness.AboveMax);
        public static CalculatedValue BelowMin => new CalculatedValue(double.MinValue, Rangeness.BelowMin);
    }

    public enum Rangeness
    {
        BelowMin,
        WithinRange,
        AboveMax
    }
}