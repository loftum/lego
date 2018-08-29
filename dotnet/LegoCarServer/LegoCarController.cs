using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Devices._4tronix;
using LCTP;
using LCTP.Routing;

namespace LegoCarServer
{
    public class LegoCarOptions
    {
        public int SteerPin { get; set; } = 0;
        public int SteerMin { get; set; } = 15;
        public int SteerMax { get; set; } = 165;
        /// <summary>
        /// 0 -> B
        /// 1 -> A
        /// </summary>
        public int MotorNumber { get; set; } = 0;

        public int MaxSpeed { get; set; } = 127;
        public int MinSpeed { get; set; } = -127;
    }

    public class 
        LegoCarController : BaseController
    {
        private readonly PiconZeroBoard _board;
        private readonly OutputPort _steer;
        private readonly MotorPort _motor;
        private readonly LegoCarOptions _options;

        public LegoCarController(PiconZeroBoard board, LegoCarOptions options)
        {
            
            _board = board;
            Get("motor/speed", GetSpeed);
            Set("motor/speed", SetSpeed);
            Set("motor/increasespeed", IncreaseSpeed);
            Set("motor/decreasespeed", DecreaseSpeed);
            Get("steer", GetSteer);
            
            Set("steer/increase", IncreaseSteer);
            Set("steer/decrease", DecreaseSteer);
            Set("steer/angle", SetSteer);

            _steer = board.Outputs[options.SteerPin];
            _steer.Type = OutputType.Servo;
            _steer.MinValue = options.SteerMin;
            _steer.MaxValue = options.SteerMax;
            _steer.Value = 90;
            _motor = board.Motors[options.MotorNumber];
            _motor.MinSpeed = options.MinSpeed;
            _motor.MaxSpeed = options.MaxSpeed;
            _options = options;
        }

        private Task<ResponseMessage> IncreaseSteer(RequestMessage request, Match match)
        {
            if (!int.TryParse(request.Content, out var delta))
            {
                return Task.FromResult(ResponseMessage.BadRequest($"Bad number: {request.Content}"));
            }
            Console.WriteLine($"Steer += {delta}");
            _steer.Value += delta;
            return Task.FromResult(ResponseMessage.Ok(_steer.Value));
        }

        private Task<ResponseMessage> DecreaseSteer(RequestMessage request, Match match)
        {
            if (!int.TryParse(request.Content, out var delta))
            {
                return Task.FromResult(ResponseMessage.BadRequest($"Bad number: {request.Content}"));
            }
            Console.WriteLine($"Steer += {delta}");
            _steer.Value -= delta;
            return Task.FromResult(ResponseMessage.Ok(_steer.Value));
        }

        private Task<ResponseMessage> DecreaseSpeed(RequestMessage request, Match match)
        {
            if (!int.TryParse(request.Content, out var delta))
            {
                return Task.FromResult(ResponseMessage.BadRequest($"Bad number: {request.Content}"));
            }
            _motor.Speed -= delta;
            return Task.FromResult(ResponseMessage.Ok(_motor.Speed));
        }

        private Task<ResponseMessage> IncreaseSpeed(RequestMessage request, Match match)
        {
            if (!int.TryParse(request.Content, out var delta))
            {
                return Task.FromResult(ResponseMessage.BadRequest($"Bad number: {request.Content}"));
            }
            _motor.Speed += delta;
            return Task.FromResult(ResponseMessage.Ok(_motor.Speed));
        }

        private Task<ResponseMessage> GetSteer(RequestMessage request, Match match)
        {
            return Task.FromResult(ResponseMessage.Ok(_steer.Value));
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

        private Task<ResponseMessage> SetSpeed(RequestMessage request, Match match)
        {
            _motor.Speed = int.Parse(request.Content);
            return Task.FromResult(ResponseMessage.Ok(_motor.Speed));
        }

        private Task<ResponseMessage> GetSpeed(RequestMessage request, Match match)
        {
            return Task.FromResult(ResponseMessage.Ok(_motor.Speed));
        }

        public override void ConnectionClosed()
        {
            _motor.Speed = 0;
            _steer.Value = 90;
        }

        public override void ConnectionOpened()
        {
            
        }
    }
}