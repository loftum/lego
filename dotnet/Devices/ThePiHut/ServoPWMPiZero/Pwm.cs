using System;
using Unosquare.PiGpio.ManagedModel;

namespace Devices.ThePiHut.ServoPWMPiZero
{
    public class Pwm
    {
        public ServoPwmBoard Board { get; }
        private readonly I2cDevice _device;
        private readonly byte _address;
        private int _onTime;
        private int _offTime;

        public int OnTime
        {
            get => _onTime;
            set
            {
                if (value < 0 || value > 4095)
                {
                    throw new ArgumentException($"Value ({value}) must be between 0 and 4096");
                }
                _device.Write(_address, (byte)(value & 0xFF));
                _device.Write((byte)(_address + 1), (byte)(value >> 8));
                _onTime = value;
            }
        }

        public int OffTime
        {
            get => _offTime;
            set
            {
                if (value < 0 || value > 4095)
                {
                    throw new ArgumentException("Value must be between 0 and 4096");
                }
                _device.Write((byte)(_address + 2), (byte)(value & 0xFF));
                _device.Write((byte)(_address + 3), (byte)(value >> 8));
                _offTime = value;
            }
        }

        public Pwm(I2cDevice device, ServoPwmBoard board, int outputNumber)
        {
            Board = board;
            _device = device;
            _address = (byte)(Registers.LED0_ON_L + 4 * outputNumber);
        }
    }
}