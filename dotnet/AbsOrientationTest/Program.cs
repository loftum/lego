using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Devices;
using Devices.Adafruit.BNO055;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

namespace AbsOrientationTest
{
    internal static class ArgsExtensions
    {
        public static bool Has(this IEnumerable<string> args, string arg)
        {
            return args.Any(a => arg.StartsWith(a, StringComparison.InvariantCultureIgnoreCase));
        }
    }

    public class AbsArguments
    {
        public bool Temp { get; }
        public bool Gyro { get; }
        public bool LinearAccel { get; }
        public bool Velocity { get; }
        public bool Accel { get; }
        public bool Mag { get; }
        public bool Euler { get; }
        public bool Quaternion { get; }
        public bool Compass { get; }

        public AbsArguments(string[] args)
        {
            Quaternion = args.Has("quaternion");
            Euler = args.Has("euler");
            Accel = args.Has("accel");
            Mag = args.Has("mag");
            LinearAccel = args.Has("linear");
            Velocity = args.Has("velocity");
            Gyro = args.Has("gyro");
            Temp = args.Has("temp");
            Compass = args.Has("compass");
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
                    Pi.Init<BootstrapWiringPi>();
                    Run(new AbsArguments(args), source.Token).Wait(source.Token);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return;
                }
            }

            Console.WriteLine("Bye!");
        }

        private static async Task Run(AbsArguments args, CancellationToken token)
        {
            while (true)
            {
                var o = new AbsOrientation(Pi.I2C);
                o.Begin(OperationMode.NDOF);

                Vector3 velocity = 0;
                var last = DateTimeOffset.UtcNow;

                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    if (args.Quaternion)
                    {
                        Console.WriteLine($"Quaternion: {o.ReadQuaternion()}");
                    }

                    if (args.Euler)
                    {
                        Console.WriteLine($"Euler: {o.ReadEulerData().ToString(" 000.00;-000.00")}");
                    }

                    if (args.Accel)
                    {
                        Console.WriteLine($"Accel: {o.ReadAccel()}");
                    }
                    if (args.Mag)
                    {
                        Console.WriteLine($"Mag: {o.ReadMag()}");
                    }

                    if (args.LinearAccel)
                    {
                        Console.WriteLine($"Linear Accel: {o.ReadLinearAccel()}");
                    }

                    if (args.Compass)
                    {
                        var mag = o.ReadMag();
                        var yaw = Math.Atan2(mag.Y, mag.X);
                        Console.WriteLine($"Compass: Rad:{yaw}, Deg:{yaw * 180 / Math.PI}");
                    }

                    if (args.Velocity)
                    {
                        var accel = o.ReadLinearAccel();
                        var now = DateTimeOffset.UtcNow;
                        velocity = velocity + accel * (now - last).TotalSeconds;
                        last = now;
                        Console.WriteLine($"Velocity: {velocity}");
                    }

                    if (args.Gyro)
                    {
                        Console.WriteLine($"Gyro: {o.ReadGyro()}");
                    }

                    if (args.Temp)
                    {
                        Console.WriteLine($"Temp: {o.ReadTemp()}");
                    }

                    await Task.Delay(100, token);
                }
            }
        }
    }
}