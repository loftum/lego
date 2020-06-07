using System;
using Devices.ABElectronics.ServoPWMPiZero;
using Lego.Core;

namespace Lego.Server
{
    public class Servo : IServo
    {
        private readonly Pwm _pwm;
        private int _value;
        public int MinPos {get; private set; }
        public int MaxPos { get; private set; }

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
                var time = MinPos + (int) ((MaxPos - MinPos) * value / 180.0);
                _pwm.OnTime = 0;
                _pwm.OffTime = time;
                _value = value;
            }
        }

        public Servo(Pwm pwm)
        {
            _pwm = pwm;
            SetPositionLimitsMs(0.7, 2.3); // pretty default for servos
            Value = 90;
        }

        public void SetPositionLimitsMs(double minMs, double maxMs)
        {
            MinPos = ToPosition(minMs);
            MaxPos = ToPosition(maxMs);
        }

        private int ToPosition(double millis)
        {
            var value = (int)(4096.0 * millis / 1000 * _pwm.Board.Frequency);
            if (value < 0 || value > 4095)
            {
                throw new ArgumentOutOfRangeException($"{value} is out of range [0, 4096>");
            }
            return value;
        }
    }

    public static class PwmExtensions
    {
        public static Servo AsServo(this Pwm pwm)
        {
            return new Servo(pwm);
        }

        public static Led AsLed(this Pwm pwm)
        {
            return new Led(pwm);
        }
    }
}