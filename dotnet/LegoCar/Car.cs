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
        private readonly int _steerPin;
        private readonly int _motorPin;
        private readonly PiconZeroBoard _picon;
        private readonly HCSR04 _sonar;
        private readonly GpioPin _lightPin;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public int Speed { get; private set; }
        public int Steer { get; private set; }
        public double DistanceInMm { get; private set; }
        public bool LightsOn { get; private set; }

        public Car(int steerPin, int motorPin, PiconZeroBoard picon, HCSR04 sonar, GpioPin lightPin)
        {
            _steerPin = steerPin;
            _motorPin = motorPin;
            _picon = picon;
            _sonar = sonar;
            lightPin.PinMode = GpioPinDriveMode.Output;
            _lightPin = lightPin;
            picon.SetOutputConfig(_steerPin, OutputType.Servo);
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
            if (Speed < 0)
            {
                Stop();
            }
            SetSpeed(Speed + 30);
        }

        public void SpeedDown()
        {
            if (Speed > 0)
            {
                Stop();
                return;
            }
            SetSpeed(Speed - 30);
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

            if (speed > 0 && DistanceInMm < DistanceLimit)
            {
                speed = 0;
            }

            Speed = speed;
            _picon.SetMotor(_motorPin, speed);
            Console.WriteLine($"Speed: {speed}");
        }

        private void SetSteer(int angle)
        {
            if (angle > 145 || angle < 35)
            {
                return;
            }
            Steer = angle;
            _picon.SetOutput(_steerPin, angle);
            Console.WriteLine($"Steer: {angle}");
        }

        public void Reset()
        {
            Stop();
            SetSteer(90);
        }

        public void SteerLeft()
        {
            SetSteer(Steer + 10);
        }

        public void SteerRight()
        {
            SetSteer(Steer - 10);
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