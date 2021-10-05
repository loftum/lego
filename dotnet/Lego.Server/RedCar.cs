using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Devices.ABElectronics.ADCPiZero;
using Devices.ABElectronics.ServoPWMPiZero;
using Devices.Adafruit.BNO055;
using Devices.Distance.Sharp.GP2Y0A41SK0F;
using Devices.PixArt;
using Devices.ThePiHut.MotoZero;
using Lego.Client;
using Lego.Core;
using Maths;
using Maths.Logging;
using Timer = System.Timers.Timer;

namespace Lego.Server
{
    public class RedCar : IRedCar
    {
        private readonly ILogger _logger = Log.For<RedCar>();
        private const double DistanceLimit = 29;
        private readonly ADCPiZeroBoard _adcBoard; 
        private readonly ServoPwmBoard _pwmBoard;
        private readonly MotoZeroBoard _motoZero;
        private readonly BNO055Sensor _imu;
        private readonly PAA5100JEQ_FlowSensor _flowSensor;
        public IServo SteerFront { get; }
        public IServo SteerBack { get; }
        public ILight LeftBlinker { get; }
        public ILight RightBlinker { get; }
        public ILight Headlights { get; }
        public ILight RearLights { get; }
        private readonly Timer _blinker = new Timer(2 * Math.PI * 100);
        private readonly DistanceSensor _frontLeftDistance;
        private readonly DistanceSensor _frontCenterDistance;
        private readonly DistanceSensor _frontRightDistance;
        
        private readonly DistanceSensor _backLeftDistance;
        private readonly DistanceSensor _backCenterDistance;
        private readonly DistanceSensor _backRightDistance;
        
        private readonly InterlockedAsyncTimer _updateTimer = new InterlockedAsyncTimer(25);
        
        public Sampled<double> FrontLeftDistance { get; } = new Sampled<double>();
        public Sampled<double> FrontCenterDistance { get; } = new Sampled<double>();
        public Sampled<double> FrontRightDistance { get; } = new Sampled<double>();

        public Sampled<double> BackLeftDistance { get; } = new Sampled<double>();
        public Sampled<double> BackCenterDistance { get; } = new Sampled<double>();
        public Sampled<double> BackRightDistance { get; } = new Sampled<double>();
        
        public Sampled<Double3> EulerAngles { get; } = new Sampled<Double3>();
        public Sampled<Quatd> Quaternion { get; } = new Sampled<Quatd>();
        public Sampled<Int2> Speed { get; } = new Sampled<Int2>();
        public Sampled<Int2> Motion { get; } = new Sampled<Int2>(); 

        public RedCar(ServoPwmBoard pwmBoard,
            MotoZeroBoard motoZero,
            ADCPiZeroBoard adcBoard,
            BNO055Sensor imu,
            PAA5100JEQ_FlowSensor flowSensor)
        {
            _pwmBoard = pwmBoard;
            _motoZero = motoZero;
            _adcBoard = adcBoard;
            _imu = imu;
            _flowSensor = flowSensor;

            _frontLeftDistance = CreateDistanceSensor(adcBoard.Inputs[0], DistanceCalculators.GP2Y0A41SK0F);
            _frontCenterDistance = CreateDistanceSensor(adcBoard.Inputs[1], DistanceCalculators.GP2Y0A02YK);
            _frontRightDistance = CreateDistanceSensor(adcBoard.Inputs[2], DistanceCalculators.GP2Y0A41SK0F);

            _backLeftDistance = CreateDistanceSensor(adcBoard.Inputs[3], DistanceCalculators.GP2Y0A41SK0F);
            _backCenterDistance = CreateDistanceSensor(adcBoard.Inputs[4], DistanceCalculators.GP2Y0A02YK);
            _backRightDistance = CreateDistanceSensor(adcBoard.Inputs[5], DistanceCalculators.GP2Y0A41SK0F);
            
            _motoZero.Motors[0].Enabled = true;
            _motoZero.Motors[1].Enabled = true;
            SteerFront = pwmBoard.Outputs[15].AsServo();
            SteerBack = pwmBoard.Outputs[0].AsServo();
            LeftBlinker = new Blinker(pwmBoard.GetOutputs(1, 9).Select(o => o.AsLed()));
            RightBlinker = new Blinker(pwmBoard.GetOutputs(4, 12).Select(o => o.AsLed()));
            RearLights = new Headlights(pwmBoard.GetOutputs(2, 3, 5, 6).Select(o => o.AsLed()));
            Headlights = new Headlights(pwmBoard.GetOutputs(10, 11, 13, 14).Select(o => o.AsLed()));
            
            
            _blinker.Elapsed += Blink;
            _blinker.Start();
            _updateTimer.Elapsed = Update;
        }

        private static DistanceSensor CreateDistanceSensor(ADCPiZeroInput input, DistanceCalculator calculator)
        {
            input.Bitrate = Bitrate._14;
            input.Pga = Pga._1;
            input.ConversionMode = ConversionMode.Continuous;
            return new DistanceSensor(input, calculator);
        }
        
        public async Task StartEngineAsync()
        {
            Reset();
            Headlights.On = true;
            RearLights.On = true;
            _updateTimer.Start();
            await _flowSensor.InitAsync(default);
            // Y is forward. X is to the right.
            _flowSensor.SetRotation(180);
        }
        
        public Task StopEngineAsync()
        {
            Reset();
            Headlights.On = false;
            RearLights.On = false;
            _updateTimer.Stop();
            _flowSensor.ShutDown();
            return Task.CompletedTask;
        }

        private async Task Update()
        {
            await ReadSensorsAsync();
            EmergencyBrakeForDistanceLimit(_motoZero.Motors[0].Speed);
        }

        private bool EmergencyBrakeForDistanceLimit(int speed)
        {
            var val = FrontCenterDistance.Value;
            if (speed > 0 &&
                (val < DistanceLimit ||
                 speed > val / 70 * MotoZeroMotor.Range))
            {
                _motoZero.Motors[0].Speed = 0;
                _motoZero.Motors[1].Speed = 0;
                return true;
            }
            else if (speed < 0 && BackCenterDistance.Value < DistanceLimit)
            {
                _motoZero.Motors[0].Speed = 0;
                _motoZero.Motors[1].Speed = 0;
                return true;
            }
            else if (speed > 0 && FrontLeftDistance.Value < DistanceLimit && SteerFront.Value > 100)
            {
                _motoZero.Motors[0].Speed = 0;
                _motoZero.Motors[1].Speed = 0;
                return true;
            }
            else if (speed > 0 && FrontRightDistance.Value < DistanceLimit && SteerFront.Value < 80)
            {
                _motoZero.Motors[0].Speed = 0;
                _motoZero.Motors[1].Speed = 0;
                return true;
            }

            return false;
        }

        private async Task ReadSensorsAsync()
        {
            FrontLeftDistance.Value = _frontLeftDistance.GetCm().Value;
            FrontCenterDistance.Value = _frontCenterDistance.GetCm().Value;
            FrontRightDistance.Value = _frontRightDistance.GetCm().Value;

            BackRightDistance.Value = _backRightDistance.GetCm().Value;
            BackCenterDistance.Value = _backCenterDistance.GetCm().Value;
            BackLeftDistance.Value = _backLeftDistance.GetCm().Value;
            
            var values = new[]
            {
                FrontLeftDistance.Value,
                FrontCenterDistance.Value,
                FrontRightDistance.Value,
                BackRightDistance.Value,
                BackCenterDistance.Value,
                BackLeftDistance.Value
            };
            _logger.Trace($"Distances: [{string.Join(", ", values)}]");
            if (_imu != null)
            {
                EulerAngles.Value = _imu.ReadEulerData();
                Quaternion.Value = _imu.ReadQuaternion();
            }

            if (_flowSensor != null)
            {
                var (x, y) = await _flowSensor.GetMotionAsync(default);
                Motion.Value = new Int2(x, y);
            }
        }

        public LegoCarState GetState()
        {
            return new LegoCarState
            {
                EulerAngles = EulerAngles.Value,
                Quaternion = Quaternion.Value,
                Throttle = Speed.Value,
                Motion = Motion.Value, // Y: forward, X: to the right
                
                Distances = new List<double>
                {
                    FrontLeftDistance.Value,
                    FrontCenterDistance.Value,
                    FrontRightDistance.Value,
                    BackRightDistance.Value,
                    BackCenterDistance.Value,
                    BackLeftDistance.Value
                }
            };
        }

        public Double3 GetEulerAngles() => EulerAngles.Value;

        public Quatd GetQuaternion() => Quaternion.Value;

        public void SetThrottle(int speed)
        {
            if (EmergencyBrakeForDistanceLimit(speed))
            {
                return;
            }
            _motoZero.Motors[0].Speed = speed;
            _motoZero.Motors[1].Speed = speed;
        }

        public void SetSteerAngle(int angle)
        {
            SteerFront.Value = angle;
            SteerBack.Value = 180 - angle;
        }

        private void Blink(object sender, ElapsedEventArgs e)
        {
            LeftBlinker.Toggle();
            RightBlinker.Toggle();
        }

        public void Reset()
        {
            _motoZero.Motors[0].Speed = 0;
            _motoZero.Motors[1].Speed = 0;
            SteerFront.Value = 90;
            SteerBack.Value = 90;
            LeftBlinker.On = false;
            RightBlinker.On = false;
        }
    }
}