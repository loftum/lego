using System;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.WiringPi;

namespace Devices.ThePiHut.MotoZero
{
    public interface IMotor
    {
        int Speed { get; set; }
    }
    
    public class MotoZeroMotor : IMotor
    {
        private readonly GpioPin _enablePin;
        private readonly GpioPin _plusPin;
        private readonly GpioPin _minusPin;
        private bool _enabled;
        private int _speed;

        private const int Range = 255;

        public int Number { get; }

        public bool Enabled
        {
            get => _enabled;
            set
            {
                var state = value ? "enabled" : "disabled";
                Console.WriteLine($"Motor {Number} {state}");
                _enablePin.Write(value);
                _enabled = value;
            }
        }

        public int Speed
        {
            get => _speed;
            set
            {
                var newValue = Sanize(value);
                if (newValue == _speed)
                {
                    return;
                }
                var abs = Math.Abs(newValue);

                switch (newValue)
                {
                    case 0:
                        _plusPin.SoftPwmValue = 0;
                        _minusPin.SoftPwmValue = 0;
                        break;
                    case int pos when pos > 0 && _speed >= 0:
                        _plusPin.SoftPwmValue = abs;
                        break;
                    case int pos when pos > 0 && _speed < 0:
                        _minusPin.SoftPwmValue = 0;
                        _plusPin.SoftPwmValue = abs;
                        break;
                    case int neg when neg < 0 && _speed <= 0:
                        _minusPin.SoftPwmValue = abs;
                        break;
                    case int neg when neg < 0 && _speed > 0:
                        _plusPin.SoftPwmValue = 0;
                        _minusPin.SoftPwmValue = abs;
                        break;
                }
                _speed = newValue;
                System.Console.WriteLine($"Speed={newValue}");
            }
        }

        private static int Sanize(int value)
        {
            if (value > Range)
            {
                return Range;
            }
            if (value < -Range)
            {
                return -Range;
            }
            return value;
        }

        public MotoZeroMotor(int number, GpioPin enablePin, GpioPin plusPin, GpioPin minusPin)
        {
            Number = number;
            enablePin.PinMode = GpioPinDriveMode.Output;
            plusPin.PinMode = GpioPinDriveMode.Output;
            plusPin.StartSoftPwm(0, Range);
            minusPin.PinMode = GpioPinDriveMode.Output;
            minusPin.StartSoftPwm(0, Range);
            _enablePin = enablePin;
            _plusPin = plusPin;
            _minusPin = minusPin;
        }

        public void Reset()
        {
            Speed = 0;
            Enabled = false;
        }
    }
}