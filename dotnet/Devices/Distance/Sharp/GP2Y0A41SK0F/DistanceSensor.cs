using Devices.ABElectronics.ADCPiZero;

namespace Devices.Distance.Sharp.GP2Y0A41SK0F
{
    /// <summary>
    /// Analog distance sensor, using analog voltage
    /// </summary>
    public class DistanceSensor : IDistanceSensor
    {
        public double MinValue { get; }
        public double MaxValue { get; }
        
        private readonly IAnalogInput _input;
        private readonly DistanceCalculator _calculator;

        public DistanceSensor(IAnalogInput input, DistanceCalculator calculator)
        {
            _input = input;
            _calculator = calculator;
            MinValue = _calculator.MinDistance;
            MaxValue = _calculator.MaxDistance;
        }

        public CalculatedValue GetCm()
        {
            var voltage = _input.ReadVoltage();
            var distance = Map(voltage);
            return distance;
        }

        private CalculatedValue Map(double voltage)
        {
            var cm = _calculator.CalculateDistance(voltage);
            return cm;
        }
    }
}
