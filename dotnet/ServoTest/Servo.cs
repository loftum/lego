using System;
using Devices._4tronix;

namespace ServoTest
{
    public class Servo : IDisposable
    {
        private readonly int _steerPin;
        private readonly PiconZeroBoard _picon;
        private int _angle;

        public int Angle
        {
            get => _angle;
            set
            {
                _angle = value;
                _picon.SetOutput(_steerPin, value);
                Console.WriteLine($"Angle: {value}");
            }
        }

        public Servo(PiconZeroBoard picon, int steerPin)
        {
            _steerPin = steerPin;
            picon.SetOutputConfig(_steerPin, OutputType.Pwm);
            _picon = picon;
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