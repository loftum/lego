using System;
using Unosquare.RaspberryIO.Abstractions;
using Unosquare.WiringPi;

namespace Devices.ThePiHut.MotoZero
{
    public class MotoZeroBoard : IDisposable
    {
        public MotoZeroMotor[] Motors { get; }

        public MotoZeroBoard(IGpioController gpio)
        {
            Motors = new[]
            {
                new MotoZeroMotor(1, (GpioPin)gpio[5], (GpioPin)gpio[24], (GpioPin)gpio[27]),
                new MotoZeroMotor(2, (GpioPin)gpio[17], (GpioPin)gpio[6], (GpioPin)gpio[22]),
                new MotoZeroMotor(3, (GpioPin)gpio[12], (GpioPin)gpio[23], (GpioPin)gpio[16]),
                new MotoZeroMotor(4, (GpioPin)gpio[25], (GpioPin)gpio[13], (GpioPin)gpio[18])
            };
        }

        public void Reset()
        {
            foreach (var motor in Motors)
            {
                motor.Reset();
            }
        }

        public void Dispose()
        {
            Reset();
        }
    }
}