using Unosquare.RaspberryIO.Gpio;

namespace Devices._4tronix
{
    public class InputPort
    {
        public int Number { get; }
        private readonly I2CDevice _device;
        private InputType _type;

        public InputType Type
        {
            get => _type;
            set
            {
                _device.WriteAddressByte(PiconZeroBoard.INCFG0 + Number, (byte)value);
                _type = value;
            }
        }

        public byte Read() => _device.ReadAddressByte(Number);

        public InputPort(I2CDevice device, int number)
        {
            _device = device;
            Number = number;
        }
    }
}