using System;
using Devices.Adafruit.BNO055;
using Unosquare.RaspberryIO;

namespace AbsOrientationTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var bus = Pi.I2C)
            {
                var o = new AbsOrientation(bus);
                

                Console.WriteLine("Hello World!");
            }
        }
    }
}
