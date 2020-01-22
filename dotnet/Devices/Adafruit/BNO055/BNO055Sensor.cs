using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Devices.Unosquare;
using Maths;
using Unosquare.RaspberryIO.Abstractions;

namespace Devices.Adafruit.BNO055
{
    public class BNO055Sensor
    {
        public const int Id = 0xA0;
        public const int DefaultI2CAddress = 0x28;
        public const int AlternativeI2CAddress = 0x29;

        private readonly II2CDevice _device;
        public const string OffsetFileName = "bno055.offsets.txt";

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

        public Double3 ReadGyro()
        {
            var bytes = ReadBytes(Registers.BNO055_GYRO_DATA_X_LSB_ADDR, 6).FixMsb();
            return bytes.ToVector3() / 16;
        }

        public Double3 ReadMag()
        {
            var bytes = ReadBytes(Registers.BNO055_MAG_DATA_X_LSB_ADDR, 6).FixMsb();
            return bytes.ToVector3() / 16;
        }

        public Quatd ReadQuaternion()
        {
            var bytes = ReadBytes(Registers.BNO055_QUATERNION_DATA_W_LSB_ADDR, 8);
            return bytes.ToQuaternion() / 16384; // 2 ^ 14 LSB
        }
        
        public Double3 ReadEulerData()
        {
            var bytes = ReadBytes(Registers.BNO055_EULER_H_LSB_ADDR, 6);

            // var shorts = bytes.ToComposedShorts();
            // Console.WriteLine($"[{string.Join(", ", shorts.Select(s => s.ToBinaryString()))}]");
            
            
            
            // Stupid bug in the chip.
            // Sometimes msb is set for no apparent reason, which makes the int16 value negative.
            // Let's MacGyver it:
            var vector = bytes.FixMsb().ToVector3();
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
        public Double3 ReadLinearAccel()
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
        public Double3 ReadRollPitchYaw()
        {
            var quaternion = ReadQuaternion();
            
            var w = quaternion.W;
            var x = quaternion.X;
            var y = quaternion.Y;
            var z = quaternion.Z;

            var roll = Math.Atan2(2 * y * w - 2 * x * z, 1 - 2 * y * y - 2 * z * z);
            var pitch = Math.Atan2(2 * x * w - 2 * y * z, 1 - 2 * x * x - 2 * z * z);
            var yaw = Math.Asin(2 * x * y + 2 * z * w);
            return new Double3(roll, pitch, yaw);
        }

        /**
         * Linear acceleration (including gravity)
         */
        public Double3 ReadAccel()
        {
            var vector = ReadBytes(Registers.BNO055_ACCEL_DATA_X_LSB_ADDR, 6)
                .FixMsb()
                .ToVector3();
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

        public BNO055Sensor(II2CBus bus, OperationMode mode)
        {
            _device = bus.AddDevice(DefaultI2CAddress);
            VerifyId();
            UnitSelection = new UnitSelection(_device);
            Begin(mode);
        }

        private void Begin(OperationMode mode)
        {
            OperationMode = OperationMode.CONFIG;
            LoadOffsets();
            Reset();
            PowerMode = PowerMode.POWER_MODE_NORMAL;
            RegisterPage = 0;
            ClockSelection = ClockSelection.External;
            OperationMode = mode;
        }

        private void LoadOffsets()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, OffsetFileName);
            if (!File.Exists(path))
            {
                return;
            }
            var text = File.ReadAllLines(path);
            if (text.Length != 22)
            {
                return;
            }

            if (!Bytes.TryParse(text, out var bytes))
            {
                return;
            }
            Console.WriteLine($"Loading offsets from {path}");
            WriteBytes(Registers.ACCEL_OFFSET_X_LSB_ADDR, bytes);
        }

        public byte[] GetSensorOffsets()
        {
            var mode = OperationMode;
            Thread.Sleep(25);
            var bytes = ReadBytes(Registers.ACCEL_OFFSET_X_LSB_ADDR, 22);
            OperationMode = mode;
            return bytes;
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

        public CalibrationStatus GetCalibration()
        {
            var calibration = ReadByte(Registers.BNO055_CALIB_STAT_ADDR);
            var system = (byte) ((calibration >> 6) & 0x03);
            var gyro = (byte) ((calibration >> 4) & 0x03);
            var accel = (byte) ((calibration >> 2) & 0x03);
            var mag = (byte) (calibration & 0x03);
            return new CalibrationStatus(system, gyro, accel, mag);
        }

        private byte ReadByte(Registers register)
        {
            return _device.ReadAddressByte((int) register);
        }
        
        private byte[] ReadBytes(Registers register, int length)
        {
            var addressStart = (int)register;
            return _device.ReadBlock(addressStart, length);
        }

        private void WriteByte(Registers register, byte value)
        {
            _device.WriteAddressByte((int)register, value);
        }
        
        private void WriteBytes(Registers register, IEnumerable<byte> values)
        {
            _device.WriteBlock((int) register, values);
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