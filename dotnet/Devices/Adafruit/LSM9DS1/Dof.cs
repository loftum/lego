using System;
using System.Threading;
using Unosquare.RaspberryIO.Gpio;
using Unosquare.RaspberryIO.Native;

namespace Devices.Adafruit.LSM9DS1
{
    /// <summary>
    /// Accelerometer, Gyro, Magnetometer
    /// </summary>
    public class Dof
    {
        private readonly I2CDevice _accelerometer;
        private readonly I2CDevice _magnometer;

        private const int AccelAddress = 0x6B;
        private const int MagAddress = 0x1E;
        private const int AccelId = 0b01101000;
        private const int MagId = 0b00111101;

        public Gyro Gyro { get; private set; }
        public Accel Accel { get; private set; }
        public Mag Mag { get; private set; }
        public Thermometer Thermometer { get; private set; }

        public Vector3 AccelValue { get; private set; }
        public Vector3 MagValue { get; private set; }
        public Vector3 GyroValue { get; private set; }
        public double TempValue { get; private set; }

        public Dof(I2CBus bus)
        {
            _accelerometer = bus.AddDevice(AccelAddress);
            _magnometer = bus.AddDevice(MagAddress);
            _accelerometer.WriteAddressByte(AccelRegisters.CTRL_REG8, 0x05);
            _magnometer.WriteAddressByte(MagRegisters.CTRL_REG2_M, 0x0c);
            Thread.Sleep(10);

            var id = _accelerometer.ReadAddressByte(AccelRegisters.WHO_AM_I_XG);
            if (id != AccelId)
            {
                throw new Exception($"Expected id {AccelId}, but got {id} for accelerometer");
            }

            id = _magnometer.ReadAddressByte(MagRegisters.WHO_AM_I_M);
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
            Mag = new Mag(_magnometer);
            Thermometer = new Thermometer(_magnometer);
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