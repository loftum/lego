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
        public int MaxSpeed { get; set; } = 127;
        public int MinSpeed { get; set; } = -127;
    }

    public class LegoCarController : BaseController
    {
        private readonly PiconZeroBoard _board;
        private readonly OutputPort _steer;
        private readonly IMotorPort _bothMotors;
        private readonly LegoCarOptions _options;

        public LegoCarController(PiconZeroBoard board, LegoCarOptions options)
        {
            
            _board = board;
            Get("motor/speed", GetSpeed);
            Set("motor/speed", SetBothMotorsSpeed);
            // 0 -> B
            // 1 -> A
            Get("motor/(\\d{1})/speed", GetMotorSpeed);
            Set("motor/(\\d{1})/speed", SetMotorSpeed);
            
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
            _bothMotors = new CompositeMotorPort(board.Motors)
            {
                MinSpeed = options.MinSpeed,
                MaxSpeed = options.MaxSpeed,
                Speed = 0
            };
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
            _bothMotors.Speed -= delta;
            return Task.FromResult(ResponseMessage.Ok(_bothMotors.Speed));
        }

        private Task<ResponseMessage> IncreaseSpeed(RequestMessage request, Match match)
        {
            if (!int.TryParse(request.Content, out var delta))
            {
                return Task.FromResult(ResponseMessage.BadRequest($"Bad number: {request.Content}"));
            }
            _bothMotors.Speed += delta;
            return Task.FromResult(ResponseMessage.Ok(_bothMotors.Speed));
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

        private Task<ResponseMessage> GetMotorSpeed(RequestMessage request, Match match)
        {
            if (!int.TryParse(match.Groups[1].Value, out var number))
            {
                return Task.FromResult(ResponseMessage.BadRequest("Bad motor number"));
            }
            return Task.FromResult(ResponseMessage.Ok(_board.Motors[number].Speed));
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
            _board.Motors[number].Speed = speed;
            return Task.FromResult(ResponseMessage.Ok(_board.Motors[number].Speed));
        }

        private Task<ResponseMessage> SetBothMotorsSpeed(RequestMessage request, Match match)
        {
            _bothMotors.Speed = int.Parse(request.Content);
            return Task.FromResult(ResponseMessage.Ok(_bothMotors.Speed));
        }

        private Task<ResponseMessage> GetSpeed(RequestMessage request, Match match)
        {
            return Task.FromResult(ResponseMessage.Ok(_bothMotors.Speed));
        }

        private void Reset()
        {
            _bothMotors.Speed = 0;
            _steer.Value = 90;
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