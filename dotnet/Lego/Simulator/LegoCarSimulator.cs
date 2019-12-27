using System.Linq;
using System.Timers;
using Devices;
using Devices.ThePiHut.MotoZero;
using Devices.ThePiHut.ServoPWMPiZero;
using Lego.Core;
using Maths;

namespace Lego.Simulator
{
    public class LegoCarSimulator : ILegoCar
    {
        public ILight LeftBlinker { get; }
        public ILight RightBlinker { get; }
        public ILight Headlights { get; }
        public IServo SteerFront { get; }
        public IServo SteerBack { get; }
        public IMotor[] Motors { get; }
        private Vector3 _orientation;
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
            var previous = _orientation;
            var speed = GetMotorSpeed(0);
            if (speed == 0 || SteerFront.Value == 0 && SteerBack.Value == 0)
            {
                return;
            }
            var diff = speed * (SteerFront.Value + SteerBack.Value) * _timer.Interval;
            _orientation = previous + new Vector3(previous.X, previous.Y, diff);
        }

        public Vector3 GetOrientation()
        {
            return _orientation;
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