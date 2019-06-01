using System;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.Swan;

namespace Devices._4tronix
{
    public class MotorPort: IMotorPort
    {
        private int _speed;
        private readonly II2CDevice _device;
        public int Number { get; }
        public int MinSpeed { get; set; } = -127;
        public int MaxSpeed { get; set; } = 127;

        public int Speed
        {
            get => _speed;
            set
            {
                if (!value.IsBetween(MinSpeed, MaxSpeed))
                {
                    return;
                }
                Console.WriteLine($"Motor {Number} Speed = {value}");
                _device.WriteAddressByte(Number, (byte)value);
                _speed = value;
            }
        }

        public MotorPort(II2CDevice device, int number)
        {
            _device = device;
            Number = number;
        }
    }
}