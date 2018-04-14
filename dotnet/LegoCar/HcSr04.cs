using System;
using System.Diagnostics;
using System.Threading;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Gpio;

namespace LegoCar
{
    public class HcSr04
    {
        private const int SpeedOfSound = 340000; // mm/s
        private readonly GpioPin _pin = Pi.Gpio.GetGpioPinByBcmPinNumber(20);
        private static readonly TimeSpan Delay = TimeSpan.FromTicks((long)(TimeSpan.TicksPerSecond * 0.00001));

        public double GetDistanceInMm()
        {
            var sw = new Stopwatch();
            var temp = new Stopwatch();
            Trig();
            _pin.PinMode = GpioPinDriveMode.Input;
            temp.Start();
            while (_pin.Read() == false && temp.ElapsedMilliseconds < 100)
            {
            }

            sw.Start();
            temp.Restart();
            while (_pin.Read() == true && temp.ElapsedMilliseconds < 100)
            {
            }

            if (temp.ElapsedMilliseconds >= 100)
            {
                // Sensor totally covered
                return 0;
            }
            sw.Stop();
            
            var distance = sw.Elapsed.TotalSeconds * SpeedOfSound / 2;
            return distance;
        }

        private void Trig()
        {
            _pin.PinMode = GpioPinDriveMode.Output;
            _pin.Write(true);
            Thread.Sleep(Delay);
            _pin.Write(false);
        }
    }
}