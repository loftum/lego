using System;
using System.Threading;
using Unosquare.RaspberryIO.Gpio;

namespace LegoCar.LSM9DS1
{
    /// <summary>
    /// Accelerometer, Gyro, Magnetometer
    /// </summary>

    public class Dof
    {
        public const int LSM9DS1_ADDRESS_ACCELGYRO = 0x6B;
        public const int LSM9DS1_ADDRESS_MAG = 0x1E;
        public const int LSM9DS1_XG_ID = 0b01101000;
        public const int LSM9DS1_MAG_ID = 0b00111101;
        // Linear Acceleration: mg per LSB
        public const double LSM9DS1_ACCEL_MG_LSB_2G = 0.061F;
        public const double LSM9DS1_ACCEL_MG_LSB_4G = 0.122F;
        public const double LSM9DS1_ACCEL_MG_LSB_8G = 0.244F;
        public const double LSM9DS1_ACCEL_MG_LSB_16G = 0.732F;
        // Magnetic Field Strength: gauss range
        public const double LSM9DS1_MAG_MGAUSS_4GAUSS = 0.14F;
        public const double LSM9DS1_MAG_MGAUSS_8GAUSS = 0.29F;
        public const double LSM9DS1_MAG_MGAUSS_12GAUSS = 0.43F;
        public const double LSM9DS1_MAG_MGAUSS_16GAUSS = 0.58F;
        // Angular Rate: dps per LSB
        public const double LSM9DS1_GYRO_DPS_DIGIT_245DPS = 0.00875F;
        public const double LSM9DS1_GYRO_DPS_DIGIT_500DPS = 0.01750F;
        public const double LSM9DS1_GYRO_DPS_DIGIT_2000DPS = 0.07000F;
        // Temperature: LSB per degree celsius
        public const int LSM9DS1_TEMP_LSB_DEGREE_CELSIUS = 8;  // 1°C = 8, 25° = 200, etc.
        public const bool MAGTYPE = true;
        public const bool XGTYPE = false;

        private bool _i2c;
        //private TwoWire* _wire;
        private byte _csm, _csxg, _mosi, _miso, _clk;
        private float _accel_mg_lsb;
        private float _mag_mgauss_lsb;
        private float _gyro_dps_digit;
        private int _lsm9dso_sensorid_accel;
        private int _lsm9dso_sensorid_mag;
        private int _lsm9dso_sensorid_gyro;
        private int _lsm9dso_sensorid_temp;
        //private Sensor _accelSensor;
        //private Sensor _magSensor;
        //private Sensor _gyroSensor;
        //private Sensor _tempSensor;

        private readonly I2CDevice _accelerometer;
        private readonly I2CDevice _magnometer;

        public Dof(I2CBus bus)
        {
            _accelerometer = bus.AddDevice(LSM9DS1_ADDRESS_ACCELGYRO);
            _magnometer = bus.AddDevice(LSM9DS1_ADDRESS_MAG);
            Reset();
        }

        public void Reset()
        {
            _accelerometer.WriteAddressByte(AccelRegisters.CTRL_REG8, 0x05);
            _magnometer.WriteAddressByte(MagRegisters.LSM9DS1_REGISTER_CTRL_REG2_M, 0x0c);
            Thread.Sleep(10);

            var id = _accelerometer.ReadAddressByte(AccelRegisters.WHO_AM_I_XG);
            if (id != LSM9DS1_XG_ID)
            {
                throw new Exception($"Expected id {LSM9DS1_XG_ID}, but got {id} for accelerometer");
            }

            id = _magnometer.ReadAddressByte(MagRegisters.LSM9DS1_REGISTER_WHO_AM_I_M);
            if (id != LSM9DS1_MAG_ID)
            {
                throw new Exception($"Expected id {LSM9DS1_MAG_ID}, but got {id} for magnometer");
            }

            // Enable gyro continuous
            _accelerometer.WriteAddressByte(AccelRegisters.CTRL_REG1_G, 0xc0);
            // Enable accelerometer continuous
            _accelerometer.WriteAddressByte(AccelRegisters.CTRL_REG5_XL, 0x38);
            _accelerometer.WriteAddressByte(AccelRegisters.CTRL_REG6_XL, 0xc0);

            // Enable mag continuous
            //_magnometer.WriteAddressByte(MagRegisters.LSM9DS1_REGISTER_CTRL_REG1_M, 0xfc); // High perf XY, 80 Hz ODR
            _magnometer.WriteAddressByte(MagRegisters.LSM9DS1_REGISTER_CTRL_REG3_M, 0x00);
            //_magnometer.WriteAddressByte(MagRegisters.LSM9DS1_REGISTER_CTRL_REG4_M, 0x0c); // High perf Z mode



        }
    }
}