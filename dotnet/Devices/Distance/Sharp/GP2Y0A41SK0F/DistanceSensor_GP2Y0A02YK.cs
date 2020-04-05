using System.Collections.Generic;
using Devices.ThePiHut.ADCPiZero;

namespace Devices.Distance.Sharp.GP2Y0A41SK0F
{
    /// <summary>
    /// 20 - 150 cm
    /// </summary>
    public class DistanceSensor_GP2Y0A02YK
    {
        private readonly IAnalogInput _input;

        private readonly DistanceCalculator _calculator = new DistanceCalculator(new [] {
            
            new PlotSample(0.78, 110),
            new PlotSample(0.84, 103),
            new PlotSample(0.90, 98),
            new PlotSample(0.96, 95),
            new PlotSample(1.03, 87),
            new PlotSample(1.1, 83.5),
            new PlotSample(1.16, 78),
            new PlotSample(1.22, 74),
            new PlotSample(1.29, 70),
            new PlotSample(1.35, 68),
            new PlotSample(1.41, 65.5),
            new PlotSample(1.48, 62.5),
            new PlotSample(1.54, 61),
            new PlotSample(1.61, 58),
            new PlotSample(1.67, 56.5),
            new PlotSample(1.73, 54),
            new PlotSample(1.80, 52),
            new PlotSample(1.86, 51.5),
            new PlotSample(1.93, 48),
            new PlotSample(1.99, 45)
        });

        public DistanceSensor_GP2Y0A02YK(IAnalogInput input)
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
            return 0;
        }
    }
}