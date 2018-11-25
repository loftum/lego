namespace Devices.Adafruit.BNO055
{
    public struct UnitSelection
    {
        public FusionDataFormat FusionDataFormat { get; set; }
        public TemperatureUnit TemperatureUnit { get; set; }
        public EulerAngleUnit EulerAngleUnit { get; set; }
        public AngularRate AngularVelocityUnit { get; set; }
        public AccelerometerUnit AccelerometerUnit { get; set; }

        public byte GetRegisterValue()
        {
            var i = (byte) FusionDataFormat << 7 |
                    (byte) TemperatureUnit << 4 |
                    (byte) EulerAngleUnit << 2 |
                    (byte) AngularVelocityUnit << 1 |
                    (byte) AccelerometerUnit << 0;
            return (byte) i;
        }

        public static UnitSelection FromRegisterValue(byte val)
        {
            return new UnitSelection
            {
                FusionDataFormat = (FusionDataFormat)(val & 0b1000_0000),
                TemperatureUnit = (TemperatureUnit)(val & 0b0000_1000),
                EulerAngleUnit = (EulerAngleUnit)(val & 0b0000_0010),
                AngularVelocityUnit = (AngularRate)(val & 0b0000_0001),
                AccelerometerUnit = (AccelerometerUnit)(val & 0b0000_0001),
            };
        }

        public static implicit operator byte(UnitSelection sel)
        {
            return sel.GetRegisterValue();
        }

        public static implicit operator UnitSelection(byte val)
        {
            return FromRegisterValue(val);
        }
    }
}