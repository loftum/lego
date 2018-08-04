using System;
using System.Threading;
using System.Threading.Tasks;
using Devices.Ultrasonic;
using Devices._4tronix;
using Unosquare.RaspberryIO.Gpio;

namespace LegoCar
{
    public class Car: IDisposable
    {
        private const double DistanceLimit = 75;
        private readonly HCSR04 _sonar;
        private readonly GpioPin _lightPin;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public double DistanceInMm { get; private set; }
        public bool LightsOn { get; private set; }
        private readonly MotorPort _motor;
        private readonly OutputPort _steer;

        public Car(int steerPin, int motorPin, PiconZeroBoard picon, HCSR04 sonar, GpioPin lightPin)
        {
            _steer = picon.Outputs[steerPin];
            _motor = picon.Motors[motorPin];
            _sonar = sonar;
            lightPin.PinMode = GpioPinDriveMode.Output;
            _lightPin = lightPin;
            picon.SetOutputConfig(steerPin, OutputType.Servo);
            Reset();
            Task.Run(() => MeasureDistance(_cancellationTokenSource.Token));
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
            }
            SetSpeed(_motor.Speed + 30);
        }

        public void SpeedDown()
        {
            if (_motor.Speed > 0)
            {
                Stop();
                return;
            }
            SetSpeed(_motor.Speed - 30);
        }

        public void Stop()
        {
            SetSpeed(0);
        }

        private void SetSpeed(int speed)
        {
            if (speed > 127 || speed < -127)
            {
                return;
            }

            if (_motor.Speed > 0 && DistanceInMm < DistanceLimit)
            {
                _motor.Speed = 0;
                return;
            }

            _motor.Speed = speed;
            Console.WriteLine($"Speed: {speed}");
        }

        private void SetSteer(int angle)
        {
            if (_steer.Value > 145 || _steer.Value < 35)
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
            SetSteer(_steer.Value + 10);
        }

        public void SteerRight()
        {
            SetSteer(_steer.Value - 10);
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
    }

    public static class BoolExtensions
    {
        public static string ToOnOff(this bool b)
        {
            return b ? "on" : "off";
        }
    }
}