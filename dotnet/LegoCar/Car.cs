using System;
using System.Threading;
using System.Threading.Tasks;
using Devices.Ultrasonic;
using Devices._4tronix;
using Unosquare.RaspberryIO.Gpio;

namespace LegoCar
{
    public class BothMotors
    {
        private readonly MotorPort _motor1;
        private readonly MotorPort _motor2;
        private int _speed;

        public int Speed
        {
            get => _speed;
            set
            {
                _motor1.Speed = value;
                _motor2.Speed = value;
                _speed = value;
            }
        }

        public BothMotors(MotorPort motor1, MotorPort motor2)
        {
            _motor1 = motor1;
            _motor2 = motor2;
        }
    }

    public class Car: IDisposable
    {
        private const double DistanceLimit = 75;
        private readonly HCSR04 _sonar;
        private readonly GpioPin _lightPin;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public double DistanceInMm { get; private set; }
        public bool LightsOn { get; private set; }
        private readonly BothMotors _motor;
        private readonly OutputPort _steer;
        private readonly bool _enableDistanceGuard;

        public Car(int steerPin, int motorPin, PiconZeroBoard picon, HCSR04 sonar, GpioPin lightPin, bool enableDistanceGuard = true)
        {
            _enableDistanceGuard = enableDistanceGuard;
            _steer = picon.Outputs[steerPin];
            _steer.Type = OutputType.Servo;
            _steer.Value = 90;
            _motor = new BothMotors(picon.Motors[0], picon.Motors[1]);
            _motor.Speed = 0;
            _sonar = sonar;
            lightPin.PinMode = GpioPinDriveMode.Output;
            _lightPin = lightPin;
            picon.SetOutputConfig(steerPin, OutputType.Servo);
            Reset();
            if (enableDistanceGuard)
            {
                Task.Run(() => MeasureDistance(_cancellationTokenSource.Token));
            }
        }

        private void MeasureDistance(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    DistanceInMm = _sonar.GetDistanceInMm();
                    if (DistanceInMm < DistanceLimit)
                    {
                        Stop();
                    }
                }
                catch
                {
                    //
                }
                finally
                {
                    Thread.Sleep(200);
                }
            }
        }

        public void SpeedUp()
        {
            if (_motor.Speed < 0)
            {
                Stop();
                return;
            }
            SetSpeed(_motor.Speed + 50);
        }

        public void SpeedDown()
        {
            if (_motor.Speed > 0)
            {
                Stop();
                return;
            }
            SetSpeed(_motor.Speed - 25);
        }

        public void Stop()
        {
            SetSpeed(0);
        }

        private void SetSpeed(int speed)
        {
            if (speed > 75)
            {
                speed = 75;
            }

            if (speed < -75)
            {
                speed = -75;
            }

            if (_enableDistanceGuard && _motor.Speed > 0 && DistanceInMm < DistanceLimit)
            {
                _motor.Speed = 0;
                return;
            }

            _motor.Speed = speed;
            Console.WriteLine($"Speed: {speed}");
        }

        private void SetSteer(int angle)
        {
            if (angle > _steer.Value && _steer.Value > 145 || angle < _steer.Value && _steer.Value < 35)
            {
                return;
            }
            _steer.Value = angle;
            Console.WriteLine($"Steer: {angle}");
        }

        public void Reset()
        {
            Stop();
            SetSteer(90);
        }

        public void SteerLeft()
        {
            SetSteer(_steer.Value - 15);
        }

        public void SteerRight()
        {
            SetSteer(_steer.Value + 15);
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }


        public void ToggleLights()
        {
            LightsOn = !LightsOn;
            Console.WriteLine($"Lights: {LightsOn.ToOnOff()}");
            _lightPin.Write(LightsOn);
        }

        public void Boost()
        {
            if (_motor.Speed >= 0)
            {
                if (_motor.Speed == 127)
                {
                    _motor.Speed = 0;
                }
                else
                {
                    _motor.Speed = 127;
                }
            }
            else
            {
                if (_motor.Speed == -127)
                {
                    _motor.Speed = 0;
                }
                else
                {
                    _motor.Speed = -127;
                }
            }
        }
    }

    public static class BoolExtensions
    {
        public static string ToOnOff(this bool b)
        {
            return b ? "on" : "off";
        }
    }
}