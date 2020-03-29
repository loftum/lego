using System;
using Devices.ThePiHut.ADCPiZero;

namespace Devices.Distance.Sharp.GP2Y0A41SK0F
{
    public class DistanceSensor_GP2Y0A41SK0F
    {
        private readonly IAnalogInput _input;
        private readonly DistanceCalculator _calculator = new DistanceCalculator(new [] {
            
            new PlotSample(0.75, 42),
            new PlotSample(0.85, 37.6),
            new PlotSample(1.035, 31.6),
            new PlotSample(1.13, 25.6),
            new PlotSample(1.22, 20.8),
            new PlotSample(1.31, 19.9),
            new PlotSample(1.41, 18.6),
            new PlotSample(1.5, 17.1),
            new PlotSample(1.6, 16.3),
            new PlotSample(1.69, 15.5),
            new PlotSample(1.79, 14.7),
            new PlotSample(1.88, 14.3),
            new PlotSample(1.98, 13.6),
            new PlotSample(2.07, 13),
            new PlotSample(2.16, 12.3),
            new PlotSample(2.26, 12),
            new PlotSample(2.35, 11.4),
            new PlotSample(2.45, 10.9),
            new PlotSample(2.54, 10.3),
            new PlotSample(2.64, 9.9),
            new PlotSample(2.73, 9.5),
            new PlotSample(2.82, 9.2),
            new PlotSample(2.91, 9)
        });

        public DistanceSensor_GP2Y0A41SK0F(IAnalogInput input)
        {
            _input = input;
        }

        public double GetCm()
        {
            var voltage = _input.ReadVoltage() * 1.465; // 0-3 V
            var distance = Map(voltage);
            return distance;
        }

        private double Map(double voltage)
        {
            if (voltage >= 3)
            {
                //Console.WriteLine($"V:{voltage}, cm:{0}");
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
