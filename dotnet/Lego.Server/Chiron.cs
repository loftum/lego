using System.Collections.Generic;
using Devices.ABElectronics.ADCPiZero;
using Devices.ABElectronics.ServoPWMPiZero;
using Devices.ThePiHut.MotoZero;
using Lego.Client;
using Lego.Core;
using Maths;
using Maths.Logging;

namespace Lego.Server
{
    public interface IChiron : ILegoCar
    {
        
    }
    
    public class Chiron : IChiron
    {
        private readonly ILogger _logger = Log.For<RedCar>();
        
        public Sampled<Int2> Speed { get; } = new Sampled<Int2>();

        private readonly IServo _steerFront;
        private readonly ADCPiZeroBoard _adcBoard; 
        private readonly ServoPwmBoard _pwmBoard;
        private readonly MotoZeroBoard _motoZero;
        private readonly InterlockedAsyncTimer _updateTimer = new InterlockedAsyncTimer(25);

        public Chiron(ServoPwmBoard pwmBoard,
            MotoZeroBoard motoZero,
            ADCPiZeroBoard adcBoard)
        {
            _pwmBoard = pwmBoard;
            _motoZero = motoZero;
            _adcBoard = adcBoard;
            _steerFront = pwmBoard.Outputs[0].AsServo();
            
            motoZero.Motors[0].Enabled = true;
            motoZero.Motors[1].Enabled = true;
            motoZero.Motors[2].Enabled = true;
            motoZero.Motors[3].Enabled = true;
        }


        public void SetThrottle(int speed)
        {
            _motoZero.Motors[0].Speed = speed;
            _motoZero.Motors[1].Speed = speed;
            _motoZero.Motors[2].Speed = speed;
            _motoZero.Motors[3].Speed = speed;
        }

        public void SetSteerAngle(int angle)
        {
            _steerFront.Value = angle;
        }

        public void Reset()
        {
            _steerFront.Value = 90;
            _motoZero.Motors[0].Speed = 0;
            _motoZero.Motors[1].Speed = 0;
            _motoZero.Motors[2].Speed = 0;
            _motoZero.Motors[3].Speed = 0;
        }

        public LegoCarState GetState()
        {
            return new LegoCarState
            {
                EulerAngles = new Double3(),
                Quaternion = new Quatd(),
                Speed = Speed.Value,
                Distances = new List<double>()
            };
        }

        public void StopEngine()
        {
            Reset();
            _updateTimer.Stop();
        }

        public void StartEngine()
        {
            Reset();
            _updateTimer.Start();
        }
    }
}