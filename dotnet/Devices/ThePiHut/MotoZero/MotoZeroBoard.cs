using System;
using Unosquare.PiGpio.ManagedModel;

namespace Devices.ThePiHut.MotoZero
{
    public class MotoZeroBoard : IDisposable
    {
        public MotoZeroMotor[] Motors { get; }

        public MotoZeroBoard(GpioPinCollection gpio)
        {
            Motors = new[]
            {
                new MotoZeroMotor(1, gpio[5], gpio[24], gpio[27]),
                new MotoZeroMotor(2, gpio[17], gpio[6], gpio[22]),
                new MotoZeroMotor(3, gpio[12], gpio[23], gpio[16]),
                new MotoZeroMotor(4, gpio[25], gpio[13], gpio[18])
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