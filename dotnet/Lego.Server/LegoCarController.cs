using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LCTP.Core;
using LCTP.Core.Routing;
using Lego.Core;

namespace Lego.Server
{
    public class LegoCarController : BaseController
    {
        private readonly ILegoCar _car;
        
        public LegoCarController(ILegoCar car)
        {
            _car = car;
            Set("input", SetInput);
            Get("state", GetState);
        }
        
        protected Task<ResponseMessage> SetInput(RequestMessage request, Match match)
        {
            if (!LegoCarInput.TryParse(request.Content, out var input))
            {
                return Task.FromResult(ResponseMessage.BadRequest("Bad input"));
            }
            _car.SetThrottle(input.Throttle);
            _car.SetSteerAngle(input.SteerAngle);
            return Task.FromResult(ResponseMessage.Ok(_car.GetState().Serialize()));
        }
        
        private Task<ResponseMessage> GetState(RequestMessage arg1, Match arg2)
        {
            return Task.FromResult(ResponseMessage.Ok(_car.GetState()));
        }
        
        private Task<ResponseMessage> SetSteer(RequestMessage request, Match match)
        {
            if (!int.TryParse(request.Content, out var angle))
            {
                return Task.FromResult(ResponseMessage.BadRequest($"Bad angle: {request.Content}"));
            }
            _car.SetSteerAngle(angle);
            return Task.FromResult(ResponseMessage.Ok());
        }

        private Task<ResponseMessage> SetMotorSpeed(RequestMessage request, Match match)
        {
            if (!int.TryParse(request.Content, out var speed))
            {
                Console.WriteLine($"Bad motor speed {speed}");
                return Task.FromResult(ResponseMessage.BadRequest("Bad motor speed"));
            }
            _car.SetThrottle(speed);
            return Task.FromResult(ResponseMessage.Ok());
        }
        
        public override void ConnectionClosed()
        {
            _car.StopEngineAsync();
        }

        public override void ConnectionOpened()
        {
            _car.StartEngineAsync();
        }
    }
}