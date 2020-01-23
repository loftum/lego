using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Devices.ThePiHut.MotoZero;
using Lego.Core;
using Maths;

namespace Lego.Server.Simulator
{
    public class LegoCarSimulator : ILegoCar
    {
        public ILight LeftBlinker { get; }
        public ILight RightBlinker { get; }
        public ILight Headlights { get; }
        public IServo SteerFront { get; }
        public IServo SteerBack { get; }
        public IMotor[] Motors { get; }
        private Double3 _eulerAngles;
        private readonly Timer _timer;

        public LegoCarSimulator(int motors)
        {
            LeftBlinker = new LightSimulator("Left blinker");
            RightBlinker = new LightSimulator("Right blinker");
            Headlights = new LightSimulator("Headlights");
            SteerFront = new ServoSimulator("Steer front");
            SteerBack = new ServoSimulator("Steer back");
            Motors = Enumerable.Range(0, motors).Select(i => new MotorSimulator($"Motor {i}")).ToArray();
            _timer = new Timer(10);
            _timer.Elapsed += Elapsed;
        }

        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            var previous = _eulerAngles;
            var speed = GetMotorSpeed(0);
            if (speed == 0 || SteerFront.Value == 0 && SteerBack.Value == 0)
            {
                return;
            }
            var diff = speed * (SteerFront.Value + SteerBack.Value) * _timer.Interval;
            _eulerAngles = previous + new Double3(previous.X, previous.Y, diff);
        }

        public LegoCarState GetState()
        {
            return new LegoCarState
            {
                EulerAngles = GetEulerAngles(),
                Distances = new List<double>()
            };
        }

        public Double3 GetEulerAngles()
        {
            return _eulerAngles;
        }

        public Quatd GetQuaternion()
        {
            var eulerAngles = _eulerAngles;
            return new Quatd(eulerAngles.X, eulerAngles.Y, eulerAngles.Z);
        }

        public int GetMotorSpeed(int motorNumber)
        {
            return Motors[motorNumber].Speed;
        }

        public void SetMotorSpeed(int motorNumber, int speed)
        {
            Motors[motorNumber].Speed = speed;
        }

        public void SetMotorSpeed(int speed)
        {
            foreach (var motor in Motors)
            {
                motor.Speed = speed;
            }
        }

        public void Reset()
        {
            SetMotorSpeed(0);
        }
    }
}