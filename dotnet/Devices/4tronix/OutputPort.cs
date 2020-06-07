using Swan;
using Unosquare.PiGpio.ManagedModel;

namespace Devices._4tronix
{
    public class OutputPort
    {
        public int Number { get; }
        private readonly I2cDevice _device;
        private OutputType _type;
        private int _value;
        public int MinValue { get; set; } = 20;
        public int MaxValue { get; set; } = 170;

        public int Value
        {
            get => _value;
            set
            {
                if (!value.IsBetween(MinValue, MaxValue))
                {
                    return;
                }
                _device.Write((byte)(PiconZeroBoard.OUTPUT0 + Number), (byte) value);
                _value = value;
            }
        }

        public OutputType Type
        {
            get => _type;
            set
            {
                _device.Write((byte)(PiconZeroBoard.OUTCFG0 + Number), (byte)value);
                _type = value;
            }
        }

        public OutputPort(I2cDevice device, int number)
        {
            _device = device;
            Number = number;
        }
    }
}