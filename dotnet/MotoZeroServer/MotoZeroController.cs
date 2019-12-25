using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Devices.ThePiHut.MotoZero;
using LCTP;
using LCTP.Core;
using LCTP.Core.Routing;

namespace MotoZeroServer
{
    public class MotoZeroController : BaseController
    {
        private readonly MotoZeroBoard _board;

        public MotoZeroController(MotoZeroBoard board)
        {
            _board = board;
            Get("motor/(\\d+)/speed", GetSpeed);
            Set("motor/(\\d+)/speed", SetSpeed);
        }

        private Task<ResponseMessage> SetSpeed(RequestMessage request, Match match)
        {
            if (int.TryParse(match.Groups[1].Value, out var motor) && int.TryParse(request.Content, out var speed))
            {
                _board.Motors[motor].Speed = speed;
                return Task.FromResult(new ResponseMessage());
            }
            return Task.FromResult(ResponseMessage.BadRequest());
        }

        private Task<ResponseMessage> GetSpeed(RequestMessage request, Match match)
        {
            var number = int.Parse(match.Groups[1].Value);
            var speed = _board.Motors[number].Speed;
            return Task.FromResult(new ResponseMessage
            {
                Content = speed.ToString()
            });
        }

        public override void ConnectionOpened()
        {
            foreach (var motor in _board.Motors)
            {
                motor.Enabled = true;
                motor.Speed = 0;
            }
        }

        public override void ConnectionClosed()
        {
            _board.Reset();
        }
    }
}