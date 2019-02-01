﻿using System;
using Unosquare.RaspberryIO.Gpio;

namespace Devices.ThePiHut.ServoPWMPiZero
{
    public class Pwm
    {
        private readonly ServoPwmBoard _board;
        private readonly I2CDevice _device;
        private readonly int _address;
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
                _device.WriteAddressByte(_address, (byte)(value & 0xFF));
                _device.WriteAddressByte(_address + 1, (byte)(value >> 8));
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
                _device.WriteAddressByte(_address + 2, (byte)(value & 0xFF));
                _device.WriteAddressByte(_address + 3, (byte)(value >> 8));
                _offTime = value;
            }
        }

        public Pwm(I2CDevice device, ServoPwmBoard board, int outputNumber)
        {
            _board = board;
            _device = device;
            _address = (int)Registers.LED0_ON_L + 4 * outputNumber;
        }

        public Servo AsServo() => new Servo(this, _board);
        public Led AsLed() => new Led(this, _board);
    }
}