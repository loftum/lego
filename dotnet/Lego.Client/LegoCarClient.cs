using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using LCTP.Core.Client;
using Lego.Core;
using Maths;
using Timer = System.Timers.Timer;

namespace Lego.Client
{
    public class LegoCarClient : IRotationProvider, IDisposable
    {
        private readonly Sampled<int> _throttle = new Sampled<int>();
        private readonly Sampled<int> _steer = new Sampled<int>();
        private readonly Timer _timer = new Timer(25);
        private int _running;
        private readonly LctpClient _client;
        private readonly ICarInput _input;

        public Switch HeadlightSwitch { get; } = new Switch();
        public Switch LeftBlinkerSwitch { get; } = new Switch();
        public Switch RightBlinkerSwitch { get; } = new Switch();
        public bool Connected => _client.Connected;
        private LegoCarState _state = new LegoCarState();

        public LegoCarClient(string host, int port, ICarInput input)
        {
            _input = input;
            _client = new LctpClient(host, port);
            _timer.Elapsed += Update;
        }

        private async void Update(object sender, ElapsedEventArgs e)
        {
            if (Interlocked.CompareExchange(ref _running, 1, 0) != 0)
            {
                return;
            }
            try
            {
                if (!await DoUpdate())
                {
                    await _client.Ping();
                }
            }
            finally
            {
                Interlocked.Exchange(ref _running, 0);
            }
        }

        private async Task<bool> DoUpdate()
        {
            var updated = false;

            _throttle.Value = await _input.GetThrottleAsync();
            if (_throttle.HasChanged)
            {
                await _client.Set("motor/speed", $"{_throttle.Value}");
            }

            _steer.Value = await _input.GetSteerAngleDegAsync();
            if (_steer.HasChanged)
            {
                await _client.Set("steer", $"{_steer.Value}");
            }

            if (HeadlightSwitch.HasChanged())
            {
                await _client.Set("headlights", "toggle");
                HeadlightSwitch.UpdateWasOn();
                updated = true;
            }
            if (LeftBlinkerSwitch.HasChanged())
            {
                await _client.Set("blinker/left", "toggle");
                LeftBlinkerSwitch.UpdateWasOn();
                updated = true;
            }
            if (RightBlinkerSwitch.HasChanged())
            {
                await _client.Set("blinker/right", "toggle");
                RightBlinkerSwitch.UpdateWasOn();
                updated = true;
            }

            var stateResult = await _client.Get("state");
            if (stateResult.StatusCode == 200)
            {
                if (LegoCarState.TryParse(stateResult.Content, out var state))
                {
                    _state = state;    
                }
                else
                {
                    Console.WriteLine($"Bad state string: {stateResult.Content}");
                }
                
            }
            else
            {
                Console.WriteLine($"Bad state status code: {stateResult.StatusCode}");
            }

            return updated;
        }
        
        public void Dispose()
        {
            _timer.Dispose();
            _client?.Dispose();
        }

        public void Connect()
        {
            _client.Connect();
            _timer.Start();
        }

        public void Disconnect()
        {
            _timer.Stop();
            _client.Disconnect();
        }

        public Double3 GetEulerAngles() => _state.EulerAngles;

        public Quatd GetQuaternion() => _state.Quaternion;
    }
}