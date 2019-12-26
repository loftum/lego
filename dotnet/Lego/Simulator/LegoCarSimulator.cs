using System.Linq;
using Devices.ThePiHut.MotoZero;
using Devices.ThePiHut.ServoPWMPiZero;
using Lego.Core;

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

        public LegoCarSimulator(int motors)
        {
            LeftBlinker = new LightSimulator("Left blinker");
            RightBlinker = new LightSimulator("Right blinker");
            Headlights = new LightSimulator("Headlights");
            SteerFront = new ServoSimulator("Steer front");
            SteerBack = new ServoSimulator("Steer back");
            Motors = Enumerable.Range(0, motors).Select(i => new MotorSimulator($"Motor {i}")).ToArray();
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