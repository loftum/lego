using System;
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
        private readonly Servo _steer;

        public LegoCarController2(ServoPwmBoard pwmBoard, MotoZeroBoard motoZero)
        {
            _pwmBoard = pwmBoard;
            _motoZero = motoZero;
            _steer = pwmBoard.Outputs[0].AsServo();

            Get("motor/speed", GetSpeed);
            Set("motor/speed", SetBothMotorsSpeed);
            Get("motor/(\\d{1})/speed", GetMotorSpeed);
            Set("motor/(\\d{1})/speed", SetMotorSpeed);
            Set("steer/angle", SetSteer);
        }

        private Task<ResponseMessage> SetSteer(RequestMessage request, Match match)
        {
            if (!int.TryParse(request.Content, out var angle))
            {
                return Task.FromResult(ResponseMessage.BadRequest($"Bad angle: {request.Content}"));
            }
            _steer.Value = angle;
            return Task.FromResult(ResponseMessage.Ok(_steer.Value));
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
            _motoZero.Motors[number].Speed = speed;
            return Task.FromResult(ResponseMessage.Ok(_motoZero.Motors[number].Speed));
        }

        private Task<ResponseMessage> SetBothMotorsSpeed(RequestMessage request, Match match)
        {
            if (!int.TryParse(request.Content, out var speed))
            {
                return Task.FromResult(ResponseMessage.BadRequest("Bad motor speed"));
            }

            _motoZero.Motors[0].Speed = speed;
            _motoZero.Motors[1].Speed = speed;
            return Task.FromResult(ResponseMessage.Ok(_motoZero.Motors[0].Speed));
        }

        private Task<ResponseMessage> GetSpeed(RequestMessage arg1, Match arg2)
        {
            throw new NotImplementedException();
        }

        public override void ConnectionClosed()
        {
            throw new NotImplementedException();
        }

        public override void ConnectionOpened()
        {
            throw new NotImplementedException();
        }
    }
}