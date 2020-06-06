using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Devices.Adafruit.BNO055;
using Devices.Distance.Sharp.GP2Y0A41SK0F;
using Devices.ThePiHut.ADCPiZero;
using Devices.ThePiHut.MotoZero;
using Devices.ThePiHut.ServoPWMPiZero;
using Lego.Client;
using Lego.Core;
using Maths;
using Timer = System.Timers.Timer;

namespace Lego.Server
{
    public class LegoCar : ILegoCar
    {
        private const double DistanceLimit = 29;
        private readonly ADCPiZeroBoard _adcBoard; 
        private readonly ServoPwmBoard _pwmBoard;
        private readonly MotoZeroBoard _motoZero;
        private readonly BNO055Sensor _imu;
        public IServo SteerFront { get; }
        public IServo SteerBack { get; }
        public ILight LeftBlinker { get; }
        public ILight RightBlinker { get; }
        public ILight Headlights { get; }
        private readonly Timer _blinker = new Timer(2 * Math.PI * 100);
        private readonly DistanceSensor _frontLeftDistance;
        private readonly DistanceSensor _frontCenterDistance;
        private readonly DistanceSensor _frontRightDistance;
        private readonly DistanceSensor _backCenterDistance;
        
        private readonly InterlockedTimer _updateTimer = new InterlockedTimer(25);
        
        public Sampled<double> FrontLeftDistance { get; } = new Sampled<double>();
        public Sampled<double> FrontCenterDistance { get; } = new Sampled<double>();
        public Sampled<double> FrontRightDistance { get; } = new Sampled<double>();
        public Sampled<double> BackCenterDistance { get; } = new Sampled<double>();
        public Sampled<Double3> EulerAngles { get; } = new Sampled<Double3>();
        public Sampled<Quatd> Quaternion { get; } = new Sampled<Quatd>();

        private int _throttle;
        private int _steer;

        public LegoCar(ServoPwmBoard pwmBoard,
            MotoZeroBoard motoZero,
            ADCPiZeroBoard adcBoard,
            BNO055Sensor imu)
        {
            _pwmBoard = pwmBoard;
            _motoZero = motoZero;
            _adcBoard = adcBoard;
            _imu = imu;

            _frontLeftDistance = CreateDistanceSensor(adcBoard.Inputs[0], DistanceCalculators.GP2Y0A41SK0F);
            _frontCenterDistance = CreateDistanceSensor(adcBoard.Inputs[1], DistanceCalculators.GP2Y0A02YK);
            _frontRightDistance = CreateDistanceSensor(adcBoard.Inputs[2], DistanceCalculators.GP2Y0A41SK0F);
            _backCenterDistance = CreateDistanceSensor(adcBoard.Inputs[4], DistanceCalculators.GP2Y0A41SK0F);
            
            _motoZero.Motors[0].Enabled = true;
            _motoZero.Motors[1].Enabled = true;
            SteerFront = pwmBoard.Outputs[15].AsServo();
            SteerBack = pwmBoard.Outputs[0].AsServo();
            LeftBlinker = new Blinker(pwmBoard.Outputs[1].AsLed());
            RightBlinker = new Blinker(pwmBoard.Outputs[4].AsLed());
            Headlights = new Headlights(new[]
                {
                    pwmBoard.Outputs[2],
                    pwmBoard.Outputs[3],
                    pwmBoard.Outputs[5],
                    pwmBoard.Outputs[6],
                }.Select(o => o.AsLed())
            );
            
            _blinker.Elapsed += Blink;
            _blinker.Start();
            _updateTimer.Elapsed += Update;
        }

        private static DistanceSensor CreateDistanceSensor(ADCPiZeroInput input, DistanceCalculator calculator)
        {
            input.Bitrate = Bitrate._14;
            input.Pga = Pga._1;
            input.ConversionMode = ConversionMode.Continuous;
            return new DistanceSensor(input, calculator);
        }
        
        public void StopEngine()
        {
            Reset();
            Headlights.On = false;
            _updateTimer.Stop();
        }

        public void StartEngine()
        {
            Reset();
            Headlights.On = true;
            _updateTimer.Start();
        }

        private void Update(object sender, ElapsedEventArgs e)
        {
            var sw = new Stopwatch();
            sw.Start();
            ReadSensors();
            sw.Stop();
        }

        private void ReadSensors()
        {
            FrontLeftDistance.Value = _frontLeftDistance.GetCm().Value;
            FrontCenterDistance.Value = _frontCenterDistance.GetCm().Value;
            FrontRightDistance.Value = _frontRightDistance.GetCm().Value;
            BackCenterDistance.Value = _backCenterDistance.GetCm().Value;
            
            if (_imu != null)
            {
                EulerAngles.Value = _imu.ReadEulerData();
                Quaternion.Value = _imu.ReadQuaternion();    
            }
        }

        public LegoCarState GetState()
        {
            return new LegoCarState
            {
                EulerAngles = EulerAngles.Value,
                Quaternion = Quaternion.Value,
                Distances = new List<double>
                {
                    FrontLeftDistance.Value,
                    FrontCenterDistance.Value,
                    FrontRightDistance.Value,
                    BackCenterDistance.Value
                }
            };
        }

        public Double3 GetEulerAngles() => EulerAngles.Value;

        public Quatd GetQuaternion() => Quaternion.Value;

        public void SetThrottle(int speed)
        {
            var val = FrontCenterDistance.Value;
            Console.WriteLine($"Front: {val}");
            if (speed > 0 &&
                (val < DistanceLimit ||
                speed > val / 65 * MotoZeroMotor.Range))
            {
                _motoZero.Motors[0].Speed = 0;
                _motoZero.Motors[1].Speed = 0;
            }
            else if (speed < 0 && BackCenterDistance.Value < DistanceLimit)
            {
                _motoZero.Motors[0].Speed = 0;
                _motoZero.Motors[1].Speed = 0;
            }
            else if (speed > 0 && FrontLeftDistance.Value < DistanceLimit && SteerFront.Value > 100)
            {
                _motoZero.Motors[0].Speed = 0;
                _motoZero.Motors[1].Speed = 0;
            }
            else if (speed > 0 && FrontRightDistance.Value < DistanceLimit && SteerFront.Value < 80)
            {
                _motoZero.Motors[0].Speed = 0;
                _motoZero.Motors[1].Speed = 0;
            }
            else
            {
                _motoZero.Motors[0].Speed = speed;
                _motoZero.Motors[1].Speed = speed;
            }
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