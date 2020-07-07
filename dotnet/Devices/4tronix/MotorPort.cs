using Maths.Logging;
using Swan;
using Unosquare.PiGpio.ManagedModel;

namespace Devices._4tronix
{
    public class MotorPort: IMotorPort
    {
        private readonly ILogger _logger;
        private int _speed;
        private readonly I2cDevice _device;
        public int Number { get; }
        public int MinSpeed { get; set; } = -127;
        public int MaxSpeed { get; set; } = 127;

        public int Speed
        {
            get => _speed;
            set
            {
                if (!value.IsBetween(MinSpeed, MaxSpeed))
                {
                    return;
                }
                _logger.Trace($"Speed = {value}");
                _device.Write((byte)Number, (byte)value);
                _speed = value;
            }
        }

        public MotorPort(I2cDevice device, int number)
        {
            _device = device;
            Number = number;
            _logger = Log.For($"Motor[{Number}]");
        }
    }
}