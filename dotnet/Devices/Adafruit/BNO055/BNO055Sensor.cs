﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Maths;
using Unosquare.PiGpio.ManagedModel;

namespace Devices.Adafruit.BNO055
{
    public class BNO055Sensor
    {
        public const byte Id = 0xA0;
        public const byte DefaultI2CAddress = 0x28;
        public const byte AlternativeI2CAddress = 0x29;

        private readonly I2cDevice _device;
        public const string OffsetFileName = "bno055.offsets.txt";

        /// <summary>
        /// 3.3 Operation modes
        /// </summary>
        public OperationMode OperationMode
        {
            get => (OperationMode) ReadByte(BNO055Registers.BNO055_OPR_MODE_ADDR);
            private set => WriteByte(BNO055Registers.BNO055_OPR_MODE_ADDR, (byte) value);
        }

        /// <summary>
        /// 3.2 Power management
        /// </summary>
        public PowerMode PowerMode
        {
            get => (PowerMode) ReadByte(BNO055Registers.BNO055_PWR_MODE_ADDR);
            private set => WriteByte(BNO055Registers.BNO055_PWR_MODE_ADDR, (byte) value);
        }

        /// <summary>
        /// 3.4 Axis remap
        /// </summary>
        public AxisRemap AxisRemap
        {
            get => (AxisRemap) ReadByte(BNO055Registers.BNO055_AXIS_MAP_CONFIG_ADDR);
            private set => WriteByte(BNO055Registers.BNO055_AXIS_MAP_CONFIG_ADDR, (byte) value);
        }

        /// <summary>
        /// 3.4 Axis remap
        /// </summary>
        public SignRemap SignRemap
        {
            get => (SignRemap)ReadByte(BNO055Registers.BNO055_AXIS_MAP_SIGN_ADDR);
            private set => WriteByte(BNO055Registers.BNO055_AXIS_MAP_SIGN_ADDR, (byte)value);
        }

        public UnitSelection UnitSelection { get; }

        public ClockSelection ClockSelection
        {
            get => (ClockSelection) (ReadByte(BNO055Registers.BNO055_SYS_TRIGGER_ADDR) & 0b1000_0000);
            set
            {
                var regValue = ReadByte(BNO055Registers.BNO055_SYS_TRIGGER_ADDR);
                var clockSelection = (byte) ((byte) value << 7);
                var newValue = regValue | clockSelection;
                WriteByte(BNO055Registers.BNO055_SYS_TRIGGER_ADDR, (byte)newValue);
            }
        }

        public int RegisterPage
        {
            get => ReadByte(BNO055Registers.BNO055_PAGE_ID_ADDR);
            private set
            {
                if (value < 0 || value > 1)
                {
                    throw new InvalidOperationException("Page must be 0 or 1");
                }
                WriteByte(BNO055Registers.BNO055_PAGE_ID_ADDR, (byte)value);
            }
        }

        public Double3 ReadGyro()
        {
            var bytes = ReadBytes(BNO055Registers.BNO055_GYRO_DATA_X_LSB_ADDR, 6).FixMsb();
            return bytes.ToVector3() / 16;
        }

        public Double3 ReadMag()
        {
            var bytes = ReadBytes(BNO055Registers.BNO055_MAG_DATA_X_LSB_ADDR, 6).FixMsb();
            return bytes.ToVector3() / 16;
        }

        public Quatd ReadQuaternion()
        {
            var bytes = ReadBytes(BNO055Registers.BNO055_QUATERNION_DATA_W_LSB_ADDR, 8);
            return bytes.FixMsb().ToQuaternion() / 16384; // 2 ^ 14 LSB
        }
        
        public Double3 ReadEulerData()
        {
            // Stupid bug in the chip.
            // Sometimes msb is set for no apparent reason, which makes the int16 value negative.
            // Let's MacGyver it:
            var buffer = ReadBytes(BNO055Registers.BNO055_EULER_H_LSB_ADDR, 6).FixMsb();
            
            var yaw = (short) (buffer[1] << 8 | buffer[0]);
            var roll = (short) (buffer[3] << 8 | buffer[2]);
            var pitch = (short) (buffer[5] << 8 | buffer[4]);
            var eulerAngles = new Double3(roll,pitch,-yaw);
            
            switch (UnitSelection.EulerAngleUnit)
            {
                case EulerAngleUnit.Radians:
                    eulerAngles /= 900;
                    return eulerAngles;
                case EulerAngleUnit.Degrees:
                    eulerAngles /= 16;
                    return eulerAngles;
                default:
                    throw new InvalidOperationException($"Unknown euler angle unit {UnitSelection.EulerAngleUnit}");
            }
        }

        /**
         * Linear acceleration (minus gravity)
         */
        public Double3 ReadLinearAccel()
        {
            var bytes = ReadBytes(BNO055Registers.BNO055_LINEAR_ACCEL_DATA_X_LSB_ADDR, 6);
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
            var vector = ReadBytes(BNO055Registers.BNO055_ACCEL_DATA_X_LSB_ADDR, 6)
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

        public BNO055Sensor(BoardPeripheralsService bus, OperationMode mode)
        {
            _device = bus.OpenI2cDevice(DefaultI2CAddress);
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
            WriteBytes(BNO055Registers.ACCEL_OFFSET_X_LSB_ADDR, bytes);
        }

        public byte[] GetSensorOffsets()
        {
            var mode = OperationMode;
            Thread.Sleep(25);
            var bytes = ReadBytes(BNO055Registers.ACCEL_OFFSET_X_LSB_ADDR, 22);
            OperationMode = mode;
            return bytes;
        }

        private void VerifyId(int retries = 1)
        {
            var id = _device.ReadByte((int) BNO055Registers.BNO055_CHIP_ID_ADDR);
            var count = 0;
            while (id != Id && count < retries)
            {
                count++;
                Thread.Sleep(100);
                id = _device.ReadByte((int)BNO055Registers.BNO055_CHIP_ID_ADDR);
            }
            if (id != Id)
            {
                throw new ApplicationException($"Unexpected id. Expected {Id}, but got {id}");
            }
        }

        private void Reset()
        {
            WriteByte(BNO055Registers.BNO055_SYS_TRIGGER_ADDR, 0x20);
            Thread.Sleep(1000);
            VerifyId(10);
            Thread.Sleep(50);
        }

        public CalibrationStatus GetCalibration()
        {
            var calibration = ReadByte(BNO055Registers.BNO055_CALIB_STAT_ADDR);
            var system = (byte) ((calibration >> 6) & 0x03);
            var gyro = (byte) ((calibration >> 4) & 0x03);
            var accel = (byte) ((calibration >> 2) & 0x03);
            var mag = (byte) (calibration & 0x03);
            return new CalibrationStatus(system, gyro, accel, mag);
        }

        private byte ReadByte(BNO055Registers register)
        {
            return _device.ReadByte((byte) register);
        }
        
        private byte[] ReadBytes(BNO055Registers register, int length)
        {
            var addressStart = (byte)register;
            return _device.ReadBlock(addressStart, length);
        }

        private void WriteByte(BNO055Registers register, byte value)
        {
            _device.Write((byte)register, value);
        }
        
        private void WriteBytes(BNO055Registers register, byte[] values)
        {
            _device.Write((byte) register, values);
        }

        public double ReadTemp()
        {
            var raw = ReadByte(BNO055Registers.BNO055_TEMP_ADDR);
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