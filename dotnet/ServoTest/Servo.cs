using System;
using Devices._4tronix;

namespace ServoTest
{
    public class Servo : IDisposable
    {
        private readonly OutputPort _output;
        
        public int Angle
        {
            get => _output.Value;
            set
            {
                _output.Value = value;
                Console.WriteLine($"Angle: {value}");
            }
        }

        public Servo(OutputPort output)
        {
            _output = output;
            _output.Type = OutputType.Servo;
            Reset();
        }

        public void Reset()
        {
            Angle = 90;
        }

        public void Dispose()
        {
            Reset();
        }
    }
}