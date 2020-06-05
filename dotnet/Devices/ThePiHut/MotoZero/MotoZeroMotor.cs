using System;
using Unosquare.PiGpio.ManagedModel;
using Unosquare.PiGpio.NativeEnums;

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
                _enablePin.Write(value ? 1 : 0);
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
                        _plusPin.SoftPwm.DutyCycle = 0;
                        _minusPin.SoftPwm.DutyCycle = 0;
                        break;
                    case int pos when pos > 0 && _speed >= 0:
                        _plusPin.SoftPwm.DutyCycle = abs;
                        break;
                    case int pos when pos > 0 && _speed < 0:
                        _minusPin.SoftPwm.DutyCycle = 0;
                        _plusPin.SoftPwm.DutyCycle = abs;
                        break;
                    case int neg when neg < 0 && _speed <= 0:
                        _minusPin.SoftPwm.DutyCycle = abs;
                        break;
                    case int neg when neg < 0 && _speed > 0:
                        _plusPin.SoftPwm.DutyCycle = 0;
                        _minusPin.SoftPwm.DutyCycle = abs;
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
            enablePin.Direction = PinDirection.Output;
            plusPin.Direction = PinDirection.Output;

            plusPin.SoftPwm.Range = Range;

            minusPin.Direction = PinDirection.Output;
            minusPin.SoftPwm.Range = Range;
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