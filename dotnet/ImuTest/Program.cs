using System;
using System.Threading;
using Devices.Adafruit.LSM9DS1;
using Unosquare.PiGpio;
using Unosquare.PiGpio.ManagedModel;

namespace ImuTest
{
    class Program
    {
        private const double G_GAIN = 0.070;
        private const double DT = 0.02;
        private const double RAD_TO_DEG = 57.29578;
        private const double AA = 0.97;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello");

            var bus = Board.Peripherals;
            //OldStyle(bus);
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

                double gyroXangle = 0.0;
                double gyroYangle = 0.0;
                double gyroZangle = 0.0;
                double AccYangle = 0.0;
                double AccXangle = 0.0;
                double CFangleX = 0.0;
                double CFangleY = 0.0;

                while (true)
                {
                    var start = DateTimeOffset.UtcNow;
                    imu.ReadAccel();
                    imu.ReadGyro();

                    var gyroAngle = imu.GyroValue * G_GAIN * DT;
                    var acc = imu.AccelValue;

                    AccXangle = (Math.Atan2(acc.Y, acc.Z) + Math.PI) * RAD_TO_DEG;
                    AccYangle = (Math.Atan2(acc.Z, acc.X) + Math.PI) * RAD_TO_DEG;

                    AccXangle -= (float)180.0;
                    if (AccYangle > 90)
                        AccYangle -= 270;
                    else
                        AccYangle += 90;

                    CFangleX = AA * (CFangleX + gyroAngle.X) + (1 - AA) * AccXangle;
                    CFangleY = AA * (CFangleY + gyroAngle.Y) + (1 - AA) * AccYangle;

                    Console.WriteLine($"X angle: {CFangleX}, Y angle: {CFangleY}");


                    while (DateTimeOffset.UtcNow < start + TimeSpan.FromSeconds(DT))
                    {
                        Thread.Sleep(1);
                    }
                }
            }
        }

        private static void OldStyle(BoardPeripheralsService bus)
        {
            using (var device = bus.OpenI2cDevice(Imu.AccelAddress))
            {
                var id = device.ReadByte(AccelRegisters.WHO_AM_I_XG);
                if (id != Imu.AccelId)
                {
                    throw new Exception($"Expected id {Imu.AccelId}, but got {id} for accelerometer");
                }

                // Enable the gyroscope
                device.Write(AccelRegisters.CTRL_REG4, 0b00111000);      // z, y, x axis enabled for gyro
                device.Write(AccelRegisters.CTRL_REG1_G, 0b10111000);    // Gyro ODR = 476Hz, 2000 dps
                device.Write(AccelRegisters.ORIENT_CFG_G, 0b10111000);   // Swap orientation 

                // Enable the accelerometer
                device.Write(AccelRegisters.CTRL_REG5_XL, 0b00111000);   // z, y, x axis enabled for accelerometer
                device.Write(AccelRegisters.CTRL_REG6_XL, 0b00101000);   // +/- 16g


                double gyroXangle = 0.0;
                double gyroYangle = 0.0;
                double gyroZangle = 0.0;
                double AccYangle = 0.0;
                double AccXangle = 0.0;
                double CFangleX = 0.0;
                double CFangleY = 0.0;


                while (true)
                {
                    var start = DateTimeOffset.UtcNow;

                    var accRaw = ReadAcc(device);
                    var gyrRaw = ReadGyr(device);

                    var rate_gyr_x = gyrRaw[0] * G_GAIN;
                    var rate_gyr_y = gyrRaw[1] * G_GAIN;
                    var rate_gyr_z = gyrRaw[2] * G_GAIN;

                    gyroXangle += rate_gyr_x * DT;
                    gyroYangle += rate_gyr_y * DT;
                    gyroZangle += rate_gyr_z * DT;

                    AccXangle = (Math.Atan2(accRaw[1], accRaw[2]) + Math.PI) * RAD_TO_DEG;
                    AccYangle = (Math.Atan2(accRaw[2], accRaw[0]) + Math.PI) * RAD_TO_DEG;

                    AccXangle -= (float)180.0;
                    if (AccYangle > 90)
                        AccYangle -= 270;
                    else
                        AccYangle += 90;

                    CFangleX = AA * (CFangleX + rate_gyr_x * DT) + (1 - AA) * AccXangle;
                    CFangleY = AA * (CFangleY + rate_gyr_y * DT) + (1 - AA) * AccYangle;

                    Console.WriteLine($"X angle: {CFangleX}, Y angle: {CFangleY}");

                    while (DateTimeOffset.UtcNow < start + TimeSpan.FromSeconds(DT))
                    {
                        Thread.Sleep(1);
                    }
                }
            }
        }

        private static int[] ReadGyr(I2cDevice device)
        {
            var numbers = new int[3];
            var block = device.ReadBlock(0x80 | AccelRegisters.OUT_X_L_G, 6);
            numbers[0] = (short) (block[0] | block[1] << 8);
            numbers[1] = (short)(block[2] | block[3] << 8);
            numbers[2] = (short)(block[4] | block[5] << 8);
            return numbers;
        }

        private static int[] ReadAcc(I2cDevice device)
        {
            var numbers = new int[3];
            var block = device.ReadBlock(0x80 | AccelRegisters.OUT_X_L_XL, 6);
            numbers[0] = (short)(block[0] | block[1] << 8);
            numbers[1] = (short)(block[2] | block[3] << 8);
            numbers[2] = (short)(block[4] | block[5] << 8);
            return numbers;
        }
    }
}
