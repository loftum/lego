using System;
using System.Threading;
using Devices.Unosquare;
using Maths;
using Unosquare.RaspberryIO.Abstractions;

namespace Devices.Adafruit.BNO055
{
    public struct CalibrationData
    {
        public byte System { get; }
        public byte Gyro { get; }
        public byte Accel { get; }
        public byte Mag { get; }

        public CalibrationData(byte system, byte gyro, byte accel, byte mag)
        {
            System = system;
            Gyro = gyro;
            Accel = accel;
            Mag = mag;
        }

        public override string ToString()
        {
            return $"Accel: {Accel}, Gyro: {Gyro}, Mag: {Mag}, System: {System}";
        }
    }
    
    public class BNO055Sensor
    {
        public const int Id = 0xA0;
        public const int DefaultI2CAddress = 0x28;
        public const int AlternativeI2CAddress = 0x29;

        private readonly II2CDevice _device;

        /// <summary>
        /// 3.3 Operation modes
        /// </summary>
        public OperationMode OperationMode
        {
            get => (OperationMode) ReadByte(Registers.BNO055_OPR_MODE_ADDR);
            private set => WriteByte(Registers.BNO055_OPR_MODE_ADDR, (byte) value);
        }

        /// <summary>
        /// 3.2 Power management
        /// </summary>
        public PowerMode PowerMode
        {
            get => (PowerMode) ReadByte(Registers.BNO055_PWR_MODE_ADDR);
            private set => WriteByte(Registers.BNO055_PWR_MODE_ADDR, (byte) value);
        }

        /// <summary>
        /// 3.4 Axis remap
        /// </summary>
        public AxisRemap AxisRemap
        {
            get => (AxisRemap) ReadByte(Registers.BNO055_AXIS_MAP_CONFIG_ADDR);
            private set => WriteByte(Registers.BNO055_AXIS_MAP_CONFIG_ADDR, (byte) value);
        }

        /// <summary>
        /// 3.4 Axis remap
        /// </summary>
        public SignRemap SignRemap
        {
            get => (SignRemap)ReadByte(Registers.BNO055_AXIS_MAP_SIGN_ADDR);
            private set => WriteByte(Registers.BNO055_AXIS_MAP_SIGN_ADDR, (byte)value);
        }

        public UnitSelection UnitSelection { get; }

        public ClockSelection ClockSelection
        {
            get => (ClockSelection) (ReadByte(Registers.BNO055_SYS_TRIGGER_ADDR) & 0b1000_0000);
            set
            {
                var regValue = ReadByte(Registers.BNO055_SYS_TRIGGER_ADDR);
                var clockSelection = (byte) ((byte) value << 7);
                var newValue = regValue | clockSelection;
                WriteByte(Registers.BNO055_SYS_TRIGGER_ADDR, (byte)newValue);
            }
        }

        public int RegisterPage
        {
            get => ReadByte(Registers.BNO055_PAGE_ID_ADDR);
            private set
            {
                if (value < 0 || value > 1)
                {
                    throw new InvalidOperationException("Page must be 0 or 1");
                }
                WriteByte(Registers.BNO055_PAGE_ID_ADDR, (byte)value);
            }
        }

        public Vector3 ReadGyro()
        {
            var bytes = ReadBytes(Registers.BNO055_GYRO_DATA_X_LSB_ADDR, 6);
            return bytes.ToVector3() / 16;
        }

        public Vector3 ReadMag()
        {
            var bytes = ReadBytes(Registers.BNO055_MAG_DATA_X_LSB_ADDR, 6);
            return bytes.ToVector3() / 16;
        }

        public Quaternion ReadQuaternion()
        {
            var bytes = ReadBytes(Registers.BNO055_QUATERNION_DATA_W_LSB_ADDR, 8);
            return bytes.ToQuaternion() / 16384; // 2 ^ 14 LSB
        }
        
        public Vector3 ReadEulerData()
        {
            var bytes = ReadBytes(Registers.BNO055_EULER_H_LSB_ADDR, 6);
            
            // Stupid bug in the chip.
            // Sometimes msb is set for no apparent reason, which makes the int16 value negative.
            // Let's MacGyver it:
            for (var ii = 1; ii < 6; ii += 2)
            {
                var msb = bytes[ii];
                if (msb > 0b_0000_1011 && msb < 0b_1000_1011)
                {
                    bytes[ii] &= 0b0111_1111; // Reset MSB
                }
            }
            
            var vector = bytes.ToVector3();
            switch (UnitSelection.EulerAngleUnit)
            {
                case EulerAngleUnit.Radians:
                    vector /= 900;
                    return vector;
                case EulerAngleUnit.Degrees:
                    vector /= 16;
                    return vector;
                default:
                    throw new InvalidOperationException($"Unknown euler angle unit {UnitSelection.EulerAngleUnit}");
            }
        }

        /**
         * Linear acceleration (minus gravity)
         */
        public Vector3 ReadLinearAccel()
        {
            var bytes = ReadBytes(Registers.BNO055_LINEAR_ACCEL_DATA_X_LSB_ADDR, 6);
            var vector = bytes.ToVector3();
            switch (UnitSelection.AccelerometerUnit)
            {
                case AccelerometerUnit.MetersPerSquareSecond:
                    return vector / 100;
                case AccelerometerUnit.MilliG:
                    return vector;
                default:
                    throw new InvalidOperationException($"Unknown accel unit");
            }
        }

        /// <summary>
        /// Returns [roll, pitch, yaw] in radians
        /// </summary>
        /// <returns></returns>
        public Vector3 ReadRollPitchYaw()
        {
            var quaternion = ReadQuaternion();
            
            var w = quaternion.W;
            var x = quaternion.X;
            var y = quaternion.Y;
            var z = quaternion.Z;

            var roll = Math.Atan2(2 * y * w - 2 * x * z, 1 - 2 * y * y - 2 * z * z);
            var pitch = Math.Atan2(2 * x * w - 2 * y * z, 1 - 2 * x * x - 2 * z * z);
            var yaw = Math.Asin(2 * x * y + 2 * z * w);
            return new Vector3(roll, pitch, yaw);
        }

        /**
         * Linear acceleration (including gravity)
         */
        public Vector3 ReadAccel()
        {
            var vector = ReadBytes(Registers.BNO055_ACCEL_DATA_X_LSB_ADDR, 6).ToVector3();
            switch (UnitSelection.AccelerometerUnit)
            {
                case AccelerometerUnit.MetersPerSquareSecond:
                    return vector / 100;
                case AccelerometerUnit.MilliG:
                    return vector;
                default:
                    throw new InvalidCastException("Unknown accel unit");
            }
        }

        private byte[] ReadBytes(Registers register, int length)
        {
            var addressStart = (int)register;
            return _device.ReadBlock(addressStart, length);
        }

        public BNO055Sensor(II2CBus bus, OperationMode mode)
        {
            _device = bus.AddDevice(DefaultI2CAddress);
            VerifyId();
            UnitSelection = new UnitSelection(_device);
            Begin(mode);
        }

        public void Begin(OperationMode mode)
        {
            OperationMode = OperationMode.CONFIG;
            Reset();
            PowerMode = PowerMode.POWER_MODE_NORMAL;
            RegisterPage = 0;
            ClockSelection = ClockSelection.External;
            OperationMode = mode;
        }

        private void VerifyId(int retries = 1)
        {
            var id = _device.ReadAddressByte((int) Registers.BNO055_CHIP_ID_ADDR);
            var count = 0;
            while (id != Id && count < retries)
            {
                count++;
                Thread.Sleep(100);
                id = _device.ReadAddressByte((int)Registers.BNO055_CHIP_ID_ADDR);
            }
            if (id != Id)
            {
                throw new ApplicationException($"Unexpected id. Expected {Id}, but got {id}");
            }
        }

        private void Reset()
        {
            WriteByte(Registers.BNO055_SYS_TRIGGER_ADDR, 0x20);
            Thread.Sleep(1000);
            VerifyId(10);
            Thread.Sleep(50);
        }

        public CalibrationData GetCalibration()
        {
            var calibration = ReadByte(Registers.BNO055_CALIB_STAT_ADDR);
            var system = (byte) ((calibration >> 6) & 0x03);
            var gyro = (byte) ((calibration >> 4) & 0x03);
            var accel = (byte) ((calibration >> 2) & 0x03);
            var mag = (byte) (calibration & 0x03);
            return new CalibrationData(system, gyro, accel, mag);
        }

        private byte ReadByte(Registers register)
        {
            return _device.ReadAddressByte((int) register);
        }

        private void WriteByte(Registers register, byte value)
        {
            _device.WriteAddressByte((int)register, value);
        }

        public double ReadTemp()
        {
            var raw = ReadByte(Registers.BNO055_TEMP_ADDR);
            switch (UnitSelection.TemperatureUnit)
            {
                case TemperatureUnit.Celsius:
                    return raw;
                case TemperatureUnit.Fahrenheit:
                    return 2 * raw;
                default:
                    throw new InvalidOperationException($"Unknown temperature unit");
            }
        }
    }
}