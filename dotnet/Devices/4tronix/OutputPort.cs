using Unosquare.RaspberryIO.Gpio;

namespace Devices._4tronix
{
    public class OutputPort
    {
        public int Number { get; }
        private readonly I2CDevice _device;
        private OutputType _type;
        private int _value;

        public int Value
        {
            get => _value;
            set
            {
                _device.WriteAddressByte(PiconZeroBoard.OUTPUT0 + Number, (byte) value);
                _value = value;
            }
        }

        public OutputType Type
        {
            get => _type;
            set
            {
                _device.WriteAddressByte(PiconZeroBoard.OUTCFG0 + Number, (byte)value);
                _type = value;
            }
        }

        public OutputPort(I2CDevice device, int number)
        {
            _device = device;
            Number = number;
        }
    }
}