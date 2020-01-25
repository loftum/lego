using System.Collections.Generic;
using Devices.ThePiHut.ADCPiZero;

namespace Devices.Distance.Sharp.GP2Y0A41SK0F
{
    public class DistanceSensor_GP2Y0A02YK
    {
        private readonly IAnalogInput _input;

        private readonly Dictionary<double, double> _samples = new Dictionary<double, double>
        {
            [.3] = 1 / .025,
             
        };

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