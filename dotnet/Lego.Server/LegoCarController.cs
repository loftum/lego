using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using LCTP.Core;
using LCTP.Core.Routing;
using Lego.Core;

namespace Lego.Server
{
    public class LegoCarController : BaseController
    {
        private readonly ILegoCar _car;
        private bool _isPerforming;
        
        public LegoCarController(ILegoCar car)
        {
            _car = car;
            Set("input", SetInput);
            Get("state", GetState);
            Perform("uturn", PerformUTurn);
        }

        private async Task<ResponseMessage> PerformUTurn(RequestMessage arg1, Match arg2)
        {
            if (_isPerforming)
            {
                return ResponseMessage.BadRequest("Performing");
            }

            _isPerforming = true;
            try
            {
                using var source = new CancellationTokenSource();
                source.CancelAfter(TimeSpan.FromSeconds(5));
                await DoPerformUturn(source.Token);

                return ResponseMessage.Ok();
            }
            catch (TaskCanceledException)
            {
                //
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                _car.Reset();
                _isPerforming = false;
            }
            return ResponseMessage.Ok();
        }

        private async Task DoPerformUturn(CancellationToken cancellationToken)
        {
            var state = _car.GetState();
            var startAngle = state.EulerAngles.Z > Math.PI ? 2 * Math.PI - state.EulerAngles.Z : state.EulerAngles.Z;
            var endAngle = startAngle + Math.PI;
            
            _car.SetThrottle(255);
            while (_car.GetState().Motion.Y < 30_000)
            {
                await Task.Delay(5, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            
            _car.SetSteerAngle(135);
            while (_car.GetState().EulerAngles.Z < endAngle)
            {
                await Task.Delay(5, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
            }
            _car.SetThrottle(0);
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