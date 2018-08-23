using System;
using System.Threading;
using Devices.Adafruit.LSM9DS1.Accelerometer;
using Devices.Adafruit.LSM9DS1.Gyroscope;
using Devices.Adafruit.LSM9DS1.Magnetometer;
using Unosquare.PiGpio.ManagedModel;


namespace Devices.Adafruit.LSM9DS1
{
    /// <summary>
    /// Accelerometer, Gyro, Magnetometer
    /// </summary>
    public class Imu : IDisposable
    {
        private readonly I2cDevice _accelerometer;
        private readonly I2cDevice _magnetometer;

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

        public Imu(BoardPeripheralsService bus)
        {
            _accelerometer = bus.OpenI2cDevice(AccelAddress);
            _magnetometer = bus.OpenI2cDevice(MagAddress);
            _accelerometer.Write(AccelRegisters.CTRL_REG8, 0x05);
            _magnetometer.Write(MagRegisters.CTRL_REG2_M, 0x0c);
            Thread.Sleep(10);

            var id = _accelerometer.ReadByte(AccelRegisters.WHO_AM_I_XG);
            if (id != AccelId)
            {
                throw new Exception($"Expected id {AccelId}, but got {id} for accelerometer");
            }

            id = _magnetometer.ReadByte(MagRegisters.WHO_AM_I_M);
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

        public void Dispose()
        {
            _accelerometer?.Dispose();
            _magnetometer?.Dispose();
        }
    }
}