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
        private readonly Servo _steer2;

        public LegoCarController2(ServoPwmBoard pwmBoard, MotoZeroBoard motoZero)
        {
            _pwmBoard = pwmBoard;
            _motoZero = motoZero;
            _motoZero.Motors[0].Enabled = true;
            _motoZero.Motors[1].Enabled = true;
            _steer = pwmBoard.Outputs[0].AsServo();
            _steer2 = pwmBoard.Outputs[1].AsServo();
            Reset();

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
            _steer2.Value = 180 - angle;
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
            _steer.Value = 90;
            _steer2.Value = 90;
        }

        private Task<ResponseMessage> GetSpeed(RequestMessage arg1, Match arg2)
        {
            throw new NotImplementedException();
        }

        public override void ConnectionClosed()
        {
            Reset();
        }

        public override void ConnectionOpened()
        {
            Reset();
        }
    }
}