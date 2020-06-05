using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Devices;
using Devices.Adafruit.BNO055;
using Maths;
using Unosquare.PiGpio;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

namespace Bno055Sensor
{
    public class Calibrator
    {
        private readonly BNO055Sensor _sensor;

        public Calibrator(BNO055Sensor sensor)
        {
            _sensor = sensor;
        }

        public async Task CalibrateAsync(CancellationToken token)
        {
            CalibrationStatus calibration;
            while (!(calibration = _sensor.GetCalibration()).IsCalibrated())
            {
                switch (calibration.CalibrateNext())
                {
                    case Calibration.Mag:
                        await CalibrateMagAsync(token);
                        break;
                    case Calibration.Gyro:
                        await CalibrateGyroAsync(token);
                        break;
                    case Calibration.Accel:
                        await CalibrateAccelAsync(token);
                        break;
                    case Calibration.System:
                        await CalibrateSystemAsync(token);
                        break;
                    default:
                        break;
                }
            }

            await StoreCalibrationProfileAsync(token);
        }

        private async Task StoreCalibrationProfileAsync(CancellationToken token)
        {
            var offsets = _sensor.GetSensorOffsets();
            Console.WriteLine("Offsets:");
            foreach (var offset in offsets)
            {
                Console.WriteLine($"{offset.ToBinaryString()} ({offset})");
            }
            Console.WriteLine();
            
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, BNO055Sensor.OffsetFileName);
            Console.WriteLine($"Storing offsets to {path}");
            await File.WriteAllTextAsync(path, string.Join(Environment.NewLine, offsets), token);
        }

        private async Task CalibrateSystemAsync(CancellationToken token)
        {
            Console.WriteLine("Waiting for system calibration");
            Console.WriteLine("...");
            CalibrationStatus calibration;
            while ((calibration = _sensor.GetCalibration()).System != 3)
            {
                if (calibration.CalibrateNext() != Calibration.System)
                {
                    Console.WriteLine($"Recalibrating {calibration.CalibrateNext()}");
                    Console.WriteLine();
                    return;
                }
                await Task.Delay(250, token);
                if (token.IsCancellationRequested)
                {
                    return;
                }
            }
            Console.WriteLine("System calibration complete!");
        }

        private async Task CalibrateAccelAsync(CancellationToken token)
        {
            Console.WriteLine("Calibrating accelerometer");
            Console.WriteLine("Place the device in 6 different stable positions for a period of few seconds to allow the accelerometer to calibrate.");
            Console.WriteLine("Make sure that there is slow movement between 2 stable positions.");
            Console.WriteLine("The 6 stable positions could be in any direction, but make sure that the device is lying at least once perpendicular to the x, y and z axis.");
            Console.WriteLine("...");
            while (_sensor.GetCalibration().Accel != 3)
            {
                await Task.Delay(250, token);
                if (token.IsCancellationRequested)
                {
                    return;
                }
            }
            Console.WriteLine("Accelerometer calibrated!");
            Console.WriteLine();
        }

        private async Task CalibrateGyroAsync(CancellationToken token)
        {
            Console.WriteLine("Calibrating gyro");
            Console.WriteLine("Place the device in a single stable position for a period of few seconds");
            Console.WriteLine("...");
            while (_sensor.GetCalibration().Gyro != 3)
            {
                await Task.Delay(250, token);
                if (token.IsCancellationRequested)
                {
                    return;
                }
            }
            Console.WriteLine("Gyro calibrated!");
            Console.WriteLine();
        }

        private async Task CalibrateMagAsync(CancellationToken token)
        {
            Console.WriteLine("Calibrating magnetometer.");
            Console.WriteLine("Make some random movements (for example: writing the number ‘8’ on air)");
            Console.WriteLine("...");
            while (_sensor.GetCalibration().Mag != 3)
            {
                await Task.Delay(250, token);
                if (token.IsCancellationRequested)
                {
                    return;
                }
            }
            Console.WriteLine("Magnetometer calibrated!");
            Console.WriteLine();
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
                Console.WriteLine($"Possible args: {string.Join(", ", AbsArguments.GetPossibleArgs())}");
                return -1;
            }
            using (var source = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, e) => source.Cancel();
                try
                {
                    Pi.Init<BootstrapWiringPi>();
                    if (a.Calibrate)
                    {
                        await CalibrateAsync(source.Token);
                    }
                    else
                    {
                        await RunAsync(a, source.Token);    
                    }
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

        private static Task CalibrateAsync(CancellationToken token)
        {
            var chip = new BNO055Sensor(Board.Peripherals, OperationMode.NDOF);
            return new Calibrator(chip).CalibrateAsync(token);
        }
        

        private static async Task RunAsync(AbsArguments args, CancellationToken token)
        {
            while (true)
            {
                var chip = new BNO055Sensor(Board.Peripherals, OperationMode.NDOF);
                Console.WriteLine(chip.UnitSelection);

                Double3 velocity = 0;
                var last = DateTimeOffset.UtcNow;
                
                while (true)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
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