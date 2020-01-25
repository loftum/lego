using System;
using System.Collections.Generic;
using System.Linq;
using Devices.ThePiHut.ADCPiZero;

namespace Devices.Distance.Sharp.GP2Y0A41SK0F
{
    public class DistanceSensor_GP2Y0A41SK0F
    {
        private readonly IAnalogInput _input;

        private readonly List<KeyValuePair<double,double>> _samples = new List<KeyValuePair<double, double>>
        {
            new KeyValuePair<double, double>(3.0, 3.5),
            new KeyValuePair<double, double>(2.63, 8),
            new KeyValuePair<double, double>(2.26, 10),
            new KeyValuePair<double, double>(2.3, 5),
            new KeyValuePair<double, double>(2, 6),
            new KeyValuePair<double, double>(1.75, 7),
            new KeyValuePair<double, double>(1.45, 8),
            new KeyValuePair<double, double>(1.4, 9),
            new KeyValuePair<double, double>(1.25, 10),
            new KeyValuePair<double, double>(1.05, 12),
            new KeyValuePair<double, double>(0.9, 14),
            new KeyValuePair<double, double>(0.8, 16),
            new KeyValuePair<double, double>(0.7, 20),
            new KeyValuePair<double, double>(0.5, 25),
            new KeyValuePair<double, double>(0.4, 30),
            new KeyValuePair<double, double>(0.35, 35),
            new KeyValuePair<double, double>(0.3, 40)
        };

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

            var above = new KeyValuePair<double, double>(double.MaxValue, 0);
            
            foreach (var pair in _samples)
            {
                if (pair.Key > voltage)
                {
                    above = pair;
                    continue;
                }
                var cm = (pair.Value + above.Value) / 2;
                Console.WriteLine($"V:{voltage}, cm:{cm}");
                return cm;
            }
            return double.MaxValue;

        }
    }
}
