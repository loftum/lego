using Unosquare.PiGpio.ManagedModel;

namespace Devices._4tronix
{
    public class InputPort
    {
        public int Number { get; }
        private readonly I2cDevice _device;
        private InputType _type;

        public InputType Type
        {
            get => _type;
            set
            {
                _device.Write((byte)(PiconZeroBoard.INCFG0 + Number), (byte)value);
                _type = value;
            }
        }

        public byte Read() => _device.ReadByte((byte)Number);

        public InputPort(I2cDevice device, int number)
        {
            _device = device;
            Number = number;
        }
    }
}