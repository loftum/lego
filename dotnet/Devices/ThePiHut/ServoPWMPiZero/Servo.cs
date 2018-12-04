using System;

namespace Devices.ThePiHut.ServoPWMPiZero
{
    public class Servo
    {
        private readonly ServoPwmBoard _board;
        private readonly Pwm _pwm;
        private int _value;
        public int MinPos {get; private set; }
        public int MaxPos { get; private set; }

        public void SetPositionLimitsMs(double minMs, double maxMs)
        {
            MinPos = ToPosition(minMs);
            MaxPos = ToPosition(maxMs);
        }

        private int ToPosition(double millis)
        {
            var value = (int) (4096.0 * millis / 1000 * _board.Frequency);
            if (value < 0 || value > 4095)
            {
                throw new ArgumentOutOfRangeException($"{value} is out of range [0, 4096>");
            }
            return value;
        }

        public int MinValue { get; set; } = 0;
        public int MaxValue { get; set; } = 180;

        public int Value
        {
            get => _value;
            set
            {
                if (value == _value || value > MaxValue || value < MinValue)
                {
                    return;
                }
                Console.WriteLine($"Value: {value}");
                var time = MinPos + (int) ((MaxPos - MinPos) * value / 180.0);
                _pwm.OnTime = 0;
                _pwm.OffTime = time;
                _value = value;
            }
        }

        public Servo(Pwm pwm, ServoPwmBoard board)
        {
            _pwm = pwm;
            _board = board;
            SetPositionLimitsMs(0.7, 2.3); // pretty default for servos
            Value = 90;
        }
    }
}