using System;
using Devices.ThePiHut.ServoPWMPiZero;

namespace Lego.Simulator
{
    public class ServoSimulator : IServo
    {
        private readonly string _name;
        private int _value;

        public ServoSimulator(string name)
        {
            _name = name;
        }

        public int Value
        {
            get => _value;
            set
            {
                _value = value;
                Console.WriteLine($"{_name}.Value = {value}");
            }
        }
    }
}