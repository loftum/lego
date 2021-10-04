using System.Threading.Tasks;
using Devices._4tronix;
using Lego.Core;
using Maths;

namespace Lego.Server
{
    public class OrangeCar : ILegoCar
    {
        private readonly PiconZeroBoard _piconZero;
        private readonly MotorPort _motor;
        private readonly OutputPort _steer;

        public OrangeCar(PiconZeroBoard piconZero)
        {
            _piconZero = piconZero;
            _motor = piconZero.Motors[0];
            _steer = piconZero.Outputs[0];
            _steer.Type = OutputType.Servo;
            const int maxAngle = 30; 
            _steer.MinValue = 90 - maxAngle;
            _steer.MaxValue = 90 + maxAngle;
        }


        public void SetThrottle(int speed)
        {
            _motor.Speed = speed;
        }

        public void SetSteerAngle(int angle)
        {
            // Servo is upside-down 
            _steer.Value = 180 - angle;
        }

        public void Reset()
        {
            _steer.Value = 90;
            _motor.Speed = 0;
        }

        public LegoCarState GetState()
        {
            return new LegoCarState
            {
                Throttle = new Int2(_motor.Speed, 0)
            };
        }

        public Task StartEngineAsync()
        {
            Reset();
            return Task.CompletedTask;
        }
        
        public Task StopEngineAsync()
        {
            Reset();
            return Task.CompletedTask;
        }
    }
}