using System;
using System.Threading;
using Devices;
using Devices.Adafruit.LSM9DS1;
using Devices.Adafruit.LSM9DS1.Accelerometer;
using Unosquare.PiGpio;
using Unosquare.PiGpio.ManagedModel;

namespace ImuTest
{
    class Program
    {
        private const double DT = 0.02;
        private const double RAD_TO_DEG = 57.29578;
        private const double AA = 0.97;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello");
            var bus = Board.Peripherals;
            NewStyle(bus);
        }

        private static void NewStyle(BoardPeripheralsService bus)
        {
            using (var imu = new Imu(bus))
            {
                imu.Accel.Settings.EnableX = true;
                imu.Accel.Settings.EnableY = true;
                imu.Accel.Settings.EnableZ = true;
                imu.Accel.Settings.SampleRate = AccelSampleRate._10Hz;
                imu.Accel.Settings.Range = AccelRange._16G;
                imu.Reset();

                var cfAngle = Vector3.Empty;

                while (true)
                {
                    var start = DateTimeOffset.UtcNow;
                    imu.ReadAccel();
                    imu.ReadGyro();

                    var gyroAngle = imu.GyroValue * DT;
                    var acc = imu.AccelValue;

                    var accAngle = new Vector3(Math.Atan2(acc.Y, acc.Z) + Math.PI, Math.Atan2(acc.Z, acc.X) + Math.PI, Math.Atan2(acc.X, acc.Y) + Math.PI) * RAD_TO_DEG;
                    accAngle.X -= 180;
                    if (accAngle.Y > 90)
                    {
                        accAngle.Y -= 270;
                    }
                    else
                    {
                        accAngle.Y += 90;
                    }

                    cfAngle = AA * (cfAngle + gyroAngle) + (1 - AA) * accAngle;

                    Console.WriteLine($"angle: {cfAngle}");

                    while (DateTimeOffset.UtcNow < start + TimeSpan.FromSeconds(DT))
                    {
                        Thread.Sleep(1);
                    }
                }
            }
        }
    }
}
