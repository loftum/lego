using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public LegoCarSimulator(int motors)
        {
            LeftBlinker = new LightSimulator("Left blinker");
            RightBlinker = new LightSimulator("Right blinker");
            Headlights = new LightSimulator("Headlights");
            SteerFront = new ServoSimulator("Steer front");
            SteerBack = new ServoSimulator("Steer back");
            Motors = Enumerable.Range(0, motors).Select(i => new MotorSimulator($"Motor {i}")).ToArray();
        }

        public LegoCarState GetState()
        {
            return new LegoCarState
            {
                EulerAngles = GetEulerAngles(),
                Distances = new List<double>(),
                Throttle = new Int2(Motors[0].Speed, 0),
                Motion = new Int2(0, 256 * Motors[0].Speed),
                Quaternion = GetQuaternion()
            };
        }
        
        public Task StartEngineAsync() => Task.CompletedTask;
        public Task StopEngineAsync() => Task.CompletedTask;

        public Double3 GetEulerAngles()
        {
            return _eulerAngles;
        }

        public Quatd GetQuaternion()
        {
            var eulerAngles = _eulerAngles;
            return new Quatd(eulerAngles.X, eulerAngles.Y, eulerAngles.Z);
        }

        public void SetThrottle(int speed)
        {
            foreach (var motor in Motors)
            {
                motor.Speed = speed;
            }
        }

        public void SetSteerAngle(int angle)
        {
            SteerFront.Value = angle;
            SteerBack.Value = angle;
        }

        public void Reset()
        {
            SetThrottle(0);
            SetSteerAngle(0);
        }
    }
}