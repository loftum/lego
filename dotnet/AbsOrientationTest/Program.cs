using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Devices.Adafruit.BNO055;
using Unosquare.RaspberryIO;

namespace AbsOrientationTest
{
    internal static class ArgsExtensions
    {
        public static bool Has(this IEnumerable<string> args, string arg)
        {
            return args.Any(a => arg.StartsWith(a, StringComparison.InvariantCultureIgnoreCase));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var source = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, e) => source.Cancel();
                try
                {
                    var quaternion = args.Has("quaternion");
                    var euler = args.Has("euler");
                    var accel = args.Has("accel");
                    var gyro = args.Has("gyro");
                    var temp = args.Has("temp");

                    Run(quaternion, euler, accel, gyro, temp, source.Token).Wait(source.Token);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return;
                }
            }

            Console.WriteLine("Bye!");
        }

        private static async Task Run(bool quaternion, bool euler, bool accel, bool gyro, bool temp,
            CancellationToken token)
        {
            using (var bus = Pi.I2C)
            {
                while (true)
                {
                    var o = new AbsOrientation(bus);
                    o.Begin(OperationMode.NDOF);

                    while (true)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }

                        if (quaternion)
                        {
                            Console.WriteLine($"Quaternion: {o.ReadQuaternion()}");
                        }

                        if (euler)
                        {
                            Console.WriteLine($"Euler: {o.ReadEulerData()}");
                        }

                        if (accel)
                        {
                            Console.WriteLine($"Accel: {o.ReadAccel()}");
                        }

                        if (gyro)
                        {
                            Console.WriteLine($"Gyro: {o.ReadGyro()}");
                        }

                        if (temp)
                        {
                            Console.WriteLine($"Temp: {o.ReadTemp()}");
                        }
                        
                        await Task.Delay(10, token);
                    }
                }
            }
        }
    }
}