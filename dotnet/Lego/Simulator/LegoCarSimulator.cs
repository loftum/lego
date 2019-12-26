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
            Motors = Enumerable.Range(0, motors).Select(i => new MotorSimulator()).ToArray();
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