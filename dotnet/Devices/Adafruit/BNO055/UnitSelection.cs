using System.Text;
using Unosquare.PiGpio.ManagedModel;

namespace Devices.Adafruit.BNO055
{
    public class UnitSelection
    {
        private readonly I2cDevice _device;
        private FusionDataFormat _fusionDataFormat;
        private TemperatureUnit _temperatureUnit;
        private EulerAngleUnit _eulerAngleUnit;
        private AngularRate _angularVelocityUnit;
        private AccelerometerUnit _accelerometerUnit;

        public UnitSelection(I2cDevice device)
        {
            _device = device;
            Load();
        }

        public FusionDataFormat FusionDataFormat
        {
            get => _fusionDataFormat;
            set
            {
                _fusionDataFormat = value;
                Save();
            }
        }

        public TemperatureUnit TemperatureUnit
        {
            get => _temperatureUnit;
            set
            {
                _temperatureUnit = value;
                Save();
            }
        }

        public EulerAngleUnit EulerAngleUnit
        {
            get => _eulerAngleUnit;
            set
            {
                _eulerAngleUnit = value;
                Save();
            }
        }

        public AngularRate AngularVelocityUnit
        {
            get => _angularVelocityUnit;
            set
            {
                _angularVelocityUnit = value;
                Save();
            }
        }

        public AccelerometerUnit AccelerometerUnit
        {
            get => _accelerometerUnit;
            set
            {
                _accelerometerUnit = value;
                Save();
            }
        }

        private byte GetRegisterValue()
        {
            var i = (byte) _fusionDataFormat << 7 |
                    (byte) _temperatureUnit << 4 |
                    (byte) _eulerAngleUnit << 2 |
                    (byte) _angularVelocityUnit << 1 |
                    (byte) _accelerometerUnit << 0;
            return (byte) i;
        }

        private void ParseRegisterValue(byte val)
        {
            _fusionDataFormat = (FusionDataFormat) ((val & 0b_1000_0000) >> 7);
            _temperatureUnit = (TemperatureUnit) ((val & 0b_0001_0000) >> 4);
            _eulerAngleUnit = (EulerAngleUnit) ((val & 0b_0000_0100) >> 2);
            _angularVelocityUnit = (AngularRate) ((val & 0b_0000_0010) >> 1);
            _accelerometerUnit = (AccelerometerUnit) (val & 0b_0000_0001);
        }
        
        private void Load()
        {
            var raw = _device.ReadByte((int) Registers.BNO055_UNIT_SEL_ADDR);
            ParseRegisterValue(raw);
        }

        private void Save()
        {
            var raw = GetRegisterValue();
            _device.Write((int)Registers.BNO055_UNIT_SEL_ADDR, raw);
        }

        public override string ToString()
        {
            return new StringBuilder()
                .AppendLine($"Fusion data format: {FusionDataFormat.ToString()}")
                .AppendLine($"Temperature unit: {TemperatureUnit.ToString()}")
                .AppendLine($"Euler angle unit: {EulerAngleUnit.ToString()}")
                .AppendLine($"Angular velocity unit: {AngularVelocityUnit.ToString()}")
                .AppendLine($"Accelerometer unit: {AccelerometerUnit.ToString()}")
                .ToString();
        }
    }
}