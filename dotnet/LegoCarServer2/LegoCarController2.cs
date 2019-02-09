using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Devices.ThePiHut.MotoZero;
using Devices.ThePiHut.ServoPWMPiZero;
using LCTP;
using LCTP.Routing;

namespace LegoCarServer2
{
    public class LegoCarController2 : BaseController
    {
        private readonly ServoPwmBoard _pwmBoard;
        private readonly MotoZeroBoard _motoZero;
        private readonly Servo _steerFront;
        private readonly Servo _steerBack;

        private readonly Led _leftBlinker;
        private readonly Led[] _headlights;
        private readonly Led _rightBlinker;

        public LegoCarController2(ServoPwmBoard pwmBoard, MotoZeroBoard motoZero)
        {
            _pwmBoard = pwmBoard;
            
            _motoZero = motoZero;
            _motoZero.Motors[0].Enabled = true;
            _motoZero.Motors[1].Enabled = true;
            _steerFront = pwmBoard.Outputs[15].AsServo();
            _steerBack = pwmBoard.Outputs[0].AsServo();
            _leftBlinker = pwmBoard.Outputs[1].AsLed();
            _rightBlinker = pwmBoard.Outputs[4].AsLed();
            _headlights = new[]
            {
                pwmBoard.Outputs[2],
                pwmBoard.Outputs[3],
                pwmBoard.Outputs[5],
                pwmBoard.Outputs[6],
            }.Select(o => o.AsLed()).ToArray();
            Reset();

            Set("headlights", SetHeadlights);
            Get("motor/speed", GetSpeed);
            Set("motor/speed", SetBothMotorsSpeed);
            Get("motor/(\\d{1})/speed", GetMotorSpeed);
            Set("motor/(\\d{1})/speed", SetMotorSpeed);
            Set("steer/angle", SetSteer);
        }

        private Task<ResponseMessage> SetHeadlights(RequestMessage request, Match match)
        {
            switch (request.Content)
            {
                case "on":
                    SetHeadLights(1.0);
                    return Task.FromResult(ResponseMessage.Ok("on"));
                case "off":
                    SetHeadLights(0);
                    return Task.FromResult(ResponseMessage.Ok("off"));
                default:
                    return Task.FromResult(ResponseMessage.BadRequest($"Unknown state {request.Content}"));
            }
        }

        private Task<ResponseMessage> SetSteer(RequestMessage request, Match match)
        {
            if (!int.TryParse(request.Content, out var angle))
            {
                return Task.FromResult(ResponseMessage.BadRequest($"Bad angle: {request.Content}"));
            }
            _steerFront.Value = angle;
            _steerBack.Value = 180 - angle;
            return Task.FromResult(ResponseMessage.Ok(_steerFront.Value));
        }

        private Task<ResponseMessage> GetMotorSpeed(RequestMessage request, Match match)
        {
            if (!int.TryParse(match.Groups[1].Value, out var number))
            {
                return Task.FromResult(ResponseMessage.BadRequest("Bad motor number"));
            }
            return Task.FromResult(ResponseMessage.Ok(_motoZero.Motors[number].Speed));
        }

        private Task<ResponseMessage> SetMotorSpeed(RequestMessage request, Match match)
        {
            if (!int.TryParse(match.Groups[1].Value, out var number))
            {
                return Task.FromResult(ResponseMessage.BadRequest("Bad motor number"));
            }

            if (!int.TryParse(request.Content, out var speed))
            {
                return Task.FromResult(ResponseMessage.BadRequest("Bad motor speed"));
            }
            _motoZero.Motors[number].Speed = speed * 2;
            return Task.FromResult(ResponseMessage.Ok(_motoZero.Motors[number].Speed));
        }

        private Task<ResponseMessage> SetBothMotorsSpeed(RequestMessage request, Match match)
        {
            if (!int.TryParse(request.Content, out var speed))
            {
                return Task.FromResult(ResponseMessage.BadRequest("Bad motor speed"));
            }

            _motoZero.Motors[0].Speed = speed * 2;
            _motoZero.Motors[1].Speed = speed * 2;
            return Task.FromResult(ResponseMessage.Ok(_motoZero.Motors[0].Speed));
        }

        private void Reset()
        {
            _motoZero.Motors[0].Speed = 0;
            _motoZero.Motors[1].Speed = 0;
            _steerFront.Value = 90;
            _steerBack.Value = 90;

        }

        private void SetHeadLights(double value)
        {
            foreach (var led in _headlights)
            {
                led.Brightness = value;
            }
        }

        private Task<ResponseMessage> GetSpeed(RequestMessage arg1, Match arg2)
        {
            throw new NotImplementedException();
        }

        public override void ConnectionClosed()
        {
            Reset();
            SetHeadLights(0);
        }

        public override void ConnectionOpened()
        {
            Reset();
            SetHeadLights(1.0);
        }
    }
}