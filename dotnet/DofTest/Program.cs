using System;
using System.Linq;
using System.Threading;
using Devices.Adafruit.LSM9DS1;
using Unosquare.PiGpio;

namespace DofTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var dof = new Dof(Board.Peripherals))
            {
                var running = true;
                Console.CancelKeyPress += (s, e) => running = false;

                var accel = args.Contains("a");
                var gyro = args.Contains("g");
                var mag = args.Contains("m");
                var temp = args.Contains("t");
                while (running)
                {
                    dof.ReadAll();
                    if (accel)
                    {
                        Console.WriteLine($"accel: {dof.AccelValue} m/s^2");
                    }

                    if (gyro)
                    {
                        Console.WriteLine($"gyro: {dof.GyroValue} degrees/s");
                    }

                    if (mag)
                    {
                        Console.WriteLine($"mag: {dof.MagValue} gauss");
                    }

                    if (temp)
                    {
                        Console.WriteLine($"temp: {dof.TempValue} C");
                    }
                    Thread.Sleep(500);
                }
            }
        }
    }
}
