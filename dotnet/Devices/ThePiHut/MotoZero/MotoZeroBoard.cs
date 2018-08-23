using System;
using Unosquare.RaspberryIO.Gpio;

namespace Devices.ThePiHut.MotoZero
{
    public class MotoZeroBoard : IDisposable
    {
        public MotoZeroMotor[] Motors { get; }

        public MotoZeroBoard(GpioController gpio)
        {
            Motors = new[]
            {
                new MotoZeroMotor(gpio.GetGpioPinByBcmPinNumber(5), gpio.GetGpioPinByBcmPinNumber(24), gpio.GetGpioPinByBcmPinNumber(27)),
                new MotoZeroMotor(gpio.GetGpioPinByBcmPinNumber(17), gpio.GetGpioPinByBcmPinNumber(6), gpio.GetGpioPinByBcmPinNumber(22)),
                new MotoZeroMotor(gpio.GetGpioPinByBcmPinNumber(12), gpio.GetGpioPinByBcmPinNumber(23), gpio.GetGpioPinByBcmPinNumber(16)),
                new MotoZeroMotor(gpio.GetGpioPinByBcmPinNumber(25), gpio.GetGpioPinByBcmPinNumber(13), gpio.GetGpioPinByBcmPinNumber(18))
            };
        }

        public void Dispose()
        {
            foreach (var motor in Motors)
            {
                motor.Dispose();
            }
        }
    }
}