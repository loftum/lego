using System;
using Unosquare.RaspberryIO.Gpio;

namespace Devices.ThePiHut.MotoZero
{
    public class MotoZeroMotor : IDisposable
    {
        private readonly GpioPin _enablePin;
        private readonly GpioPin _plusPin;
        private readonly GpioPin _minusPin;
        private bool _enabled;
        private int _speed;

        private const int Range = 255;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                _enablePin.Write(value);
                if (value)
                {
                    _plusPin.StartSoftPwm(0, Range);
                    _minusPin.StartSoftPwm(0, Range);
                }
                _enabled = value;
            }
        }

        public int Speed
        {
            get => _speed;
            set
            {
                var abs = Math.Abs(value);
                if (abs > Range)
                {
                    return;
                }

                switch (value)
                {
                    case 0:
                        _plusPin.SoftPwmValue = 0;
                        _minusPin.SoftPwmValue = 0;
                        break;
                    case int pos when pos > 0 && _speed >= 0:
                        _plusPin.SoftPwmValue = value;
                        break;
                    case int pos when pos > 0 && _speed < 0:
                        _minusPin.SoftPwmValue = 0;
                        _plusPin.SoftPwmValue = value;
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


        public MotoZeroMotor(GpioPin enablePin, GpioPin plusPin, GpioPin minusPin)
        {
            enablePin.PinMode = GpioPinDriveMode.Output;
            plusPin.PinMode = GpioPinDriveMode.Output;
            minusPin.PinMode = GpioPinDriveMode.Output;
            _enablePin = enablePin;
            _plusPin = plusPin;
            _minusPin = minusPin;
        }

        public void Dispose()
        {
            Speed = 0;
            Enabled = false;
        }
    }
}