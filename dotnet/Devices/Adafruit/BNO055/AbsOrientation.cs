using System;
using System.Linq;
using System.Threading;
using Unosquare.RaspberryIO.Gpio;

namespace Devices.Adafruit.BNO055
{
    public class AbsOrientation
    {
        public const int Id = 0xA0;
        public const int DefaultI2CAddress = 0x28;
        public const int AlternativeI2CAddress = 0x29;

        private readonly I2CDevice _device;

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

        public UnitSelection UnitSelection
        {
            get => ReadByte(Registers.BNO055_UNIT_SEL_ADDR);
            private set => WriteByte(Registers.BNO055_UNIT_SEL_ADDR, value);
        }

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
            return bytes.ToQuaternion();
        }

        public Vector3 ReadEulerData()
        {
            var bytes = ReadBytes(Registers.BNO055_EULER_H_LSB_ADDR, 6);
            // Stupid bug in the chip.
            // Whenever pitch > 0, pitch MSB is set erroneously
            var msb = (short) (bytes[5] << 8 | bytes[4]);
            if (msb > 2880 || msb < -2880)
            //if (msb > 12 && msb < 139) // between 00001011 and 10001011
            {
                bytes[5] &= 0b0111_1111;
            }

            Console.WriteLine($"[{string.Join(", ", bytes.Select(b => Convert.ToString(b, 2)))}]");
            var vector = bytes.ToVector3();
            return bytes.ToVector3() / 16.0;
        }

        public Vector3 ReadLinearAccel()
        {
            var bytes = ReadBytes(Registers.BNO055_LINEAR_ACCEL_DATA_X_LSB_ADDR, 6);
            return bytes.ToVector3() / 100;
        }

        public Vector3 ReadAccel()
        {
            var bytes = ReadBytes(Registers.BNO055_ACCEL_DATA_X_LSB_ADDR, 6);
            return bytes.ToVector3() / 100;
        }

        private byte[] ReadBytes(Registers register, int length)
        {
            var addressStart = (int)register;
            var bytes = new byte[length];
            for (var ii = 0; ii < bytes.Length; ii++)
            {
                bytes[ii] = _device.ReadAddressByte(addressStart + ii);
            }
            return bytes;
        }

        public AbsOrientation(I2CBus bus)
        {
            _device = bus.AddDevice(DefaultI2CAddress);
            VerifyId();
        }

        public void Begin(OperationMode mode)
        {
            OperationMode = OperationMode.CONFIG;
            Reset();
            PowerMode = PowerMode.POWER_MODE_NORMAL;
            RegisterPage = 0;
            UnitSelection = new UnitSelection
            {
                AccelerometerUnit = AccelerometerUnit.MetersPerSquareSecond,
                AngularVelocityUnit = AngularRate.DegreesPerSecond,
                EulerAngleUnit = EulerAngleUnit.Degrees,
                TemperatureUnit = TemperatureUnit.Celsius,
                FusionDataFormat = FusionDataFormat.Windows
            };
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
            return ReadByte(Registers.BNO055_TEMP_ADDR);  
        }
    }
}