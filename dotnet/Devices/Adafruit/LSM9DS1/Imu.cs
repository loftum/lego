using System;
using System.Threading;
using Devices.Adafruit.LSM9DS1.Accelerometer;
using Devices.Adafruit.LSM9DS1.Gyroscope;
using Devices.Adafruit.LSM9DS1.Magnetometer;
using Maths;
using Unosquare.RaspberryIO.Abstractions;


namespace Devices.Adafruit.LSM9DS1
{
    /// <summary>
    /// Accelerometer, Gyro, Magnetometer
    /// </summary>
    public class Imu
    {
        private readonly II2CDevice _accelerometer;
        private readonly II2CDevice _magnetometer;

        public const int AccelAddress = 0x6B;
        public const int MagAddress = 0x1E;
        public const int AccelId = 0b01101000; // 0x68
        public const int MagId = 0b00111101; // 0x3d

        public Gyro Gyro { get; private set; }
        public Accel Accel { get; private set; }
        public Mag Mag { get; private set; }
        public Thermometer Thermometer { get; private set; }

        public Vector3 AccelValue { get; private set; }
        public Vector3 MagValue { get; private set; }
        public Vector3 GyroValue { get; private set; }
        public double TempValue { get; private set; }

        public Imu(II2CBus bus)
        {
            _accelerometer = bus.AddDevice(AccelAddress);
            _magnetometer = bus.AddDevice(MagAddress);
            _accelerometer.WriteAddressByte(AccelRegisters.CTRL_REG8, 0x05);
            _magnetometer.WriteAddressByte(MagRegisters.CTRL_REG2_M, 0x0c);
            Thread.Sleep(10);

            var id = _accelerometer.ReadAddressByte(AccelRegisters.WHO_AM_I_XG);
            if (id != AccelId)
            {
                throw new Exception($"Expected id {AccelId}, but got {id} for accelerometer");
            }

            id = _magnetometer.ReadAddressByte(MagRegisters.WHO_AM_I_M);
            if (id != MagId)
            {
                throw new Exception($"Expected id {MagId}, but got {id} for magnometer");
            }
            Reset();
        }

        public void Reset()
        {
            Gyro = new Gyro(_accelerometer);
            Accel = new Accel(_accelerometer);
            Mag = new Mag(_magnetometer);
            Thermometer = new Thermometer(_magnetometer);
        }

        public void ReadAll()
        {
            ReadAccel();
            ReadMag();
            ReadGyro();
            ReadTemp();
        }

        public void ReadAccel()
        {
            AccelValue = Accel.Read();
        }

        public void ReadMag()
        {
            MagValue = Mag.Read();
        }

        public void ReadGyro()
        {
            GyroValue = Gyro.Read();
        }

        public void ReadTemp()
        {
            TempValue = Thermometer.Read();
        }
    }
}