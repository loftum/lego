using System;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Devices.ThePiHut.ADCPiZero;
using Devices.ThePiHut.MotoZero;
using Devices.ThePiHut.ServoPWMPiZero;

namespace Lego.Core
{
    public class LegoCar
    {
        private const double DistanceLimit = 1.1;
        private readonly ADCPiZeroBoard _adcBoard; 
        private readonly ServoPwmBoard _pwmBoard;
        private readonly MotoZeroBoard _motoZero;
        public Servo SteerFront {get; }
        public Servo SteerBack {get; }

        public Blinker LeftBlinker { get; }
        public Blinker RightBlinker { get; }
        public Headlights Headlights { get; }
        private readonly Timer _blinker = new Timer(2 * Math.PI * 100);
        private readonly ADCPiZeroInput _frontDistance;
        private readonly Task _distanceTask;
        private double _lastDistance;
        public double Distance { get; private set; }

        public LegoCar(ServoPwmBoard pwmBoard, MotoZeroBoard motoZero, ADCPiZeroBoard adcBoard)
        {
            _pwmBoard = pwmBoard;
            _motoZero = motoZero;
            _adcBoard = adcBoard;
            _frontDistance = adcBoard.Inputs[0];
            _frontDistance.Bitrate = Bitrate._16;
            _frontDistance.ConversionMode = ConversionMode.Continuous;
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
            _distanceTask = Task.Run(MeasureDistance);
        }
        
        private void Blink(object sender, ElapsedEventArgs e)
        {
            LeftBlinker.Toggle();
            RightBlinker.Toggle();
        }
        
        private async Task MeasureDistance()
        {
            while(true)
            {
                _lastDistance = Distance;
                Distance = _frontDistance.ReadVoltage();
                if (Distance > DistanceLimit && _lastDistance > DistanceLimit)
                {
                    Console.WriteLine($"Distance: {Distance}");
                    if (_motoZero.Motors[0].Speed > 0)
                    {
                        _motoZero.Motors[0].Speed = 0;
                    }
                    if (_motoZero.Motors[1].Speed > 0)
                    {
                        _motoZero.Motors[1].Speed = 0;
                    }
                }
                await Task.Delay(75);
            }
        }
    }
}