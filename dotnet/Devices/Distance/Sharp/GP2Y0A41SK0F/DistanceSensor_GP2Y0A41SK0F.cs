using System;
using Devices.ThePiHut.ADCPiZero;

namespace Devices.Distance.Sharp.GP2Y0A41SK0F
{
    public class DistanceSensor_GP2Y0A41SK0F
    {
        private readonly IAnalogInput _input;
        private readonly DistanceCalculator _calculator = new DistanceCalculator(new [] {
            new PlotSample(3.0, 3.5),
            new PlotSample(2.63, 8),
            new PlotSample(2.26, 10),
            new PlotSample(2.3, 5),
            new PlotSample(2, 6),
            new PlotSample(1.75, 7),
            new PlotSample(1.45, 8),
            new PlotSample(1.4, 9),
            new PlotSample(1.25, 10),
            new PlotSample(1.05, 12),
            new PlotSample(0.9, 14),
            new PlotSample(0.8, 16),
            new PlotSample(0.7, 20),
            new PlotSample(0.5, 25),
            new PlotSample(0.4, 30),
            new PlotSample(0.35, 35),
            new PlotSample(0.3, 40)
        });

        public DistanceSensor_GP2Y0A41SK0F(IAnalogInput input)
        {
            _input = input;
        }

        public double GetCm()
        {
            var voltage = _input.ReadVoltage() * 1.465; // 0-3 V
            if (voltage < 0)
            {
                return double.MaxValue;
            }
            var distance = Map(voltage);
            return distance;
        }

        private double Map(double voltage)
        {
            if (voltage >= 3)
            {
                Console.WriteLine($"V:{voltage}, cm:{0}");
                return 0;
            }

            if (voltage < 0.3)
            {
                return double.MaxValue;
            }

            return _calculator.CalculateDistance(voltage);
        }
    }
}
