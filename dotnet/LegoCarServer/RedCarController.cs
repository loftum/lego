using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LCTP.Core;
using Lego.Core;
using Lego.Server;

namespace LegoCarServer
{
    public class RedCarController : LegoCarController
    {
        private readonly IRedCar _car;

        public RedCarController(IRedCar car) : base(car)
        {
            _car = car;
            Set("blinker/(.+)", SetBlinker);
            Set("headlights", SetHeadlights);
            Get("orientation/euler", GetEulerAngles);
            Get("orientation/quaternion", GetEulerAngles);
        }

        private Task<ResponseMessage> GetEulerAngles(RequestMessage request, Match match)
        {
            return Task.FromResult(ResponseMessage.Ok(_car.GetEulerAngles()));
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
    }
}