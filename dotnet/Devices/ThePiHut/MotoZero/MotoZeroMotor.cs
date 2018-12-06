using System;
using Unosquare.RaspberryIO.Gpio;

namespace Devices.ThePiHut.MotoZero
{
    public class MotoZeroMotor
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
                value = Sanize(value);
                if (value == _speed)
                {
                    return;
                }
                var abs = Math.Abs(value);

                switch (value)
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

                Console.WriteLine($"Speed={value}");
                _speed = value;
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