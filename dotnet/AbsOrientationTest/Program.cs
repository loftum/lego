using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Devices.Adafruit.BNO055;
using Maths;
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
        public bool RollPitchYaw { get; }
        public bool Calibration { get; }

        private static readonly PropertyInfo[] Properties = typeof(AbsArguments)
            .GetProperties()
            .ToArray();

        public bool IsValid()
        {
            return Properties.Any(p => (bool)p.GetValue(this));
        }

        public IEnumerable<string> GetPossibleArgs()
        {
            return Properties.Select(p => p.Name);
        }

        public AbsArguments(string[] args)
        {
            Quaternion = args.Has("quaternion");
            RollPitchYaw = args.Has("rpy") || args.Has("rollpitchyaw");
            Euler = args.Has("euler");
            Accel = args.Has("accel");
            Mag = args.Has("mag");
            LinearAccel = args.Has("linear");
            Velocity = args.Has("velocity");
            Gyro = args.Has("gyro");
            Temp = args.Has("temp");
            Compass = args.Has("compass");
            Calibration = args.Has("calibration");
        }
    }

    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var a = new AbsArguments(args);
            if (!a.IsValid())
            {
                Console.WriteLine($"Usage: {Process.GetCurrentProcess().ProcessName} <args>");
                Console.WriteLine($"Possible args: {string.Join(", ", a.GetPossibleArgs())}");
                return -1;
            }
            using (var source = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, e) => source.Cancel();
                try
                {
                    Pi.Init<BootstrapWiringPi>();
                    await RunAsync(a, source.Token);
                    return 0;
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Bye!");
                    return 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.GetType().FullName);
                    Console.WriteLine(e.Message);
                    Console.WriteLine(e.StackTrace);
                    return -1;
                }
            }
        }

        private static async Task RunAsync(AbsArguments args, CancellationToken token)
        {
            while (true)
            {
                var chip = new BNO055Sensor(Pi.I2C, OperationMode.NDOF);
                Console.WriteLine(chip.UnitSelection);

                Vector3 velocity = 0;
                var last = DateTimeOffset.UtcNow;

                
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    if (args.Calibration)
                    {
                        // accel.x, accel.y, accel.z, accel, gyro, mag, system
                        var calibration = chip.GetCalibration();
                        var accel = chip.ReadAccel();
                        Console.WriteLine($"System: {calibration.System}");
                        Console.WriteLine($"Accel: {accel}, Cal: {calibration}");
                    }
                    
                    if (args.Quaternion)
                    {
                        Console.WriteLine($"Quaternion: {chip.ReadQuaternion()}");
                    }
                    
                    if (args.RollPitchYaw)
                    {
                        var rpy = chip.ReadRollPitchYaw();
                        Console.WriteLine($"Roll pitch yaw: rad {rpy}  deg {rpy * 180 / Math.PI}");
                    }

                    if (args.Euler)
                    {
                        Console.WriteLine($"Euler: {chip.ReadEulerData().ToString(" 000.00;-000.00")}");
                    }

                    if (args.Accel)
                    {
                        Console.WriteLine($"Accel: {chip.ReadAccel()}");
                    }
                    if (args.Mag)
                    {
                        Console.WriteLine($"Mag: {chip.ReadMag()}");
                    }

                    if (args.LinearAccel)
                    {
                        Console.WriteLine($"Linear Accel: {chip.ReadLinearAccel()}");
                    }

                    if (args.Compass)
                    {
                        var mag = chip.ReadMag();
                        var yaw = Math.Atan2(mag.Y, mag.X);
                        Console.WriteLine($"Compass: Rad:{yaw}, Deg:{yaw * 180 / Math.PI}");
                    }

                    if (args.Velocity)
                    {
                        var accel = chip.ReadLinearAccel();
                        var now = DateTimeOffset.UtcNow;
                        velocity = velocity + accel * (now - last).TotalSeconds;
                        last = now;
                        Console.WriteLine($"Velocity: {velocity}");
                    }

                    if (args.Gyro)
                    {
                        Console.WriteLine($"Gyro: {chip.ReadGyro()}");
                    }

                    if (args.Temp)
                    {
                        Console.WriteLine($"Temp: {chip.ReadTemp()}");
                    }

                    await Task.Delay(100, token);
                }
            }
        }
    }
}