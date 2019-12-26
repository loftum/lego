using System;
using Devices.ThePiHut.MotoZero;

namespace Lego.Simulator
{
    public class MotorSimulator : IMotor
    {
        private readonly string _name;
        private int _speed;

        public MotorSimulator(string name)
        {
            _name = name;
        }

        public int Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                Console.WriteLine($"{_name}.Speed = {value}");
            } 
        }
    }
}