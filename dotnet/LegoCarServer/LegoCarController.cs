using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LCTP.Core;
using LCTP.Core.Routing;
using Lego.Core;

namespace LegoCarServer
{
    public class LegoCarController : BaseController
    {
        private readonly ILegoCar _car;

        public LegoCarController(ILegoCar car)
        {
            _car = car;
            //_car.Reset();
            Set("blinker/(.+)", SetBlinker);
            Set("headlights", SetHeadlights);
            Set("motor/speed", SetBothMotorsSpeed);
            Get("motor/(\\d{1})/speed", GetMotorSpeed);
            Set("motor/(\\d{1})/speed", SetMotorSpeed);
            Set("steer/angle", SetSteer);
            Get("orientation", GetOrientation);
            Get("state", GetState);
        }

        private Task<ResponseMessage> GetState(RequestMessage arg1, Match arg2)
        {
            return Task.FromResult(ResponseMessage.Ok(_car.GetState()));
        }

        private Task<ResponseMessage> GetOrientation(RequestMessage request, Match match)
        {
            return Task.FromResult(ResponseMessage.Ok(_car.GetOrientation()));
        }

        private Task<ResponseMessage> SetBlinker(RequestMessage request, Match match)
        {
            var which = match.Groups[1].Value;
            switch (which)
            {
                case "left":
                    _car.LeftBlinker.On = request.Content == "on" || request.Content == "toggle" && !_car.LeftBlinker.On;
                    _car.RightBlinker.On = false;
                    return Task.FromResult(ResponseMessage.Ok("on"));
                case "right":
                    _car.RightBlinker.On = request.Content == "on" || request.Content == "toggle" && !_car.RightBlinker.On;
                    _car.LeftBlinker.On = false;
                    return Task.FromResult(ResponseMessage.Ok("on"));
                default:
                    return Task.FromResult(ResponseMessage.BadRequest($"Unknown blinker {which}"));
            }
        }

        private Task<ResponseMessage> SetHeadlights(RequestMessage request, Match match)
        {
            switch (request.Content)
            {
                case "on":
                    _car.Headlights.On = true;
                    return Task.FromResult(ResponseMessage.Ok());
                case "off":
                    _car.Headlights.On = false;
                    return Task.FromResult(ResponseMessage.Ok());
                case "toggle":
                    _car.Headlights.Toggle();
                    return Task.FromResult(ResponseMessage.Ok());
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
            
            _car.SteerFront.Value = angle;
            _car.SteerBack.Value = 180 - angle;
            return Task.FromResult(ResponseMessage.Ok());
        }

        private Task<ResponseMessage> GetMotorSpeed(RequestMessage request, Match match)
        {
            if (!int.TryParse(match.Groups[1].Value, out var number))
            {
                return Task.FromResult(ResponseMessage.BadRequest("Bad motor number"));
            }
            return Task.FromResult(ResponseMessage.Ok(_car.GetMotorSpeed(number)));
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
            _car.SetMotorSpeed(number, speed);
            return Task.FromResult(ResponseMessage.Ok());
        }

        private Task<ResponseMessage> SetBothMotorsSpeed(RequestMessage request, Match match)
        {
            if (!int.TryParse(request.Content, out var speed))
            {
                return Task.FromResult(ResponseMessage.BadRequest("Bad motor speed"));
            }
            _car.SetMotorSpeed(speed);
            return Task.FromResult(ResponseMessage.Ok());
        }

        public override void ConnectionClosed()
        {
            _car.Reset();
            _car.Headlights.On = false;
        }

        public override void ConnectionOpened()
        {
            _car.Reset();
            _car.Headlights.On = true;
        }
    }
}