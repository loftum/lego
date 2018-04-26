using System;
using System.Threading;
using Unosquare.RaspberryIO.Gpio;

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

        // Temperature: LSB per degree celsius
        public const int LSM9DS1_TEMP_LSB_DEGREE_CELSIUS = 8;  // 1°C = 8, 25° = 200, etc.

        private AccelRange _accelRange = AccelRange._2G;
        public AccelRange AccelRange
        {
            get => _accelRange;
            set
            {
                int regValue = _accelerometer.ReadAddressByte(AccelRegisters.CTRL_REG6_XL);
                regValue &= ~0b00011000;
                regValue |= value.RegMask;
                _accelerometer.WriteAddressByte(AccelRegisters.CTRL_REG6_XL, (byte)regValue);
                _accelRange = value;
            }
        }

        private MagGain _magGain = MagGain._4GAUSS;
        public MagGain MagGain
        {
            get => _magGain;
            set
            {
                int regValue = _magnometer.ReadAddressByte(MagRegisters.CTRL_REG2_M);
                regValue &= ~0b01100000;
                regValue |= value.RegMask;
                _magnometer.WriteAddressByte(MagRegisters.CTRL_REG2_M, (byte)regValue);
                _magGain = value;
            }
        }

        private GyroScale _gyroScale = GyroScale._245DPS;
        public GyroScale GyroScale
        {
            get => _gyroScale;
            set
            {
                int regValue = _accelerometer.ReadAddressByte(AccelRegisters.CTRL_REG1_G);
                regValue &= ~0b00011000;
                regValue |= value.RegMask;
                _accelerometer.WriteAddressByte(AccelRegisters.CTRL_REG1_G, (byte)regValue);
                _gyroScale = value;
            }
        }

        public Vector3 AccelValue { get; private set; }
        public Vector3 MagValue { get; private set; }
        public Vector3 GyroValue { get; private set; }
        public float TempValue { get; private set; }

        public Dof(I2CBus bus)
        {
            _accelerometer = bus.AddDevice(AccelAddress);
            _magnometer = bus.AddDevice(MagAddress);
            Reset();
        }

        public void Reset()
        {
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

            // Enable gyro continuous
            _accelerometer.WriteAddressByte(AccelRegisters.CTRL_REG1_G, 0xc0);
            // Enable accelerometer continuous
            _accelerometer.WriteAddressByte(AccelRegisters.CTRL_REG5_XL, 0x38);
            _accelerometer.WriteAddressByte(AccelRegisters.CTRL_REG6_XL, 0xc0);

            // Enable mag continuous
            //_magnometer.WriteAddressByte(MagRegisters.CTRL_REG1_M, 0xfc); // High perf XY, 80 Hz ODR
            _magnometer.WriteAddressByte(MagRegisters.CTRL_REG3_M, 0x00);
            //_magnometer.WriteAddressByte(MagRegisters.CTRL_REG4_M, 0x0c); // High perf Z mode

            AccelRange = AccelRange._2G;
            MagGain = MagGain._4GAUSS;
            GyroScale = GyroScale._245DPS;
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
            AccelValue = _accelerometer.ReadAddressBytes(0x80 | AccelRegisters.OUT_X_L_XL, 6)
                             .ToVector3() * AccelRange.MgPerLsb * Constants.SENSORS_GRAVITY_STANDARD / 1000;
        }

        public void ReadMag()
        {
            MagValue = _magnometer.ReadAddressBytes(0x80 | MagRegisters.OUT_X_L_M, 6)
                .ToVector3() * MagGain.Gain / 1000;
        }

        public void ReadGyro()
        {
            GyroValue = _accelerometer.ReadAddressBytes(0x80 | AccelRegisters.OUT_X_L_G, 6)
                .ToVector3() * GyroScale.DpsPerLsb;
        }

        public void ReadTemp()
        {
            TempValue = _accelerometer.ReadAddressBytes(AccelRegisters.TEMP_OUT_L, 2)
                .ToUshort() / LSM9DS1_TEMP_LSB_DEGREE_CELSIUS;
        }
    }
}