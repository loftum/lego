using System.Collections.Generic;
using System.Linq;

namespace Devices._4tronix
{
    public class CompositeMotorPort : IMotorPort
    {
        private readonly IList<MotorPort> _ports;

        private int _minSpeed;
        public int MinSpeed
        {
            get => _minSpeed;
            set
            {
                foreach (var port in _ports)
                {
                    port.MinSpeed = value;
                }
                _minSpeed = value;
            }
        }

        private int _maxSpeed;
        public int MaxSpeed
        {
            get => _maxSpeed;
            set
            {
                foreach (var port in _ports)
                {
                    port.MaxSpeed = value;
                }
                _maxSpeed = value;
            }
        }

        private int _speed;

        public int Speed
        {
            get => _speed;
            set
            {
                foreach (var port in _ports)
                {
                    port.Speed = value;
                }
                _speed = value;
            }
        }

        public CompositeMotorPort(IEnumerable<MotorPort> ports)
        {
            _ports = ports.ToList();
            MinSpeed = -127;
            MaxSpeed = 127;
            Speed = 0;
        }
    }
}