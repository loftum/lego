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
    public class LegoCarClient : IRotationProvider, ILegoCarStateProvider, IDisposable
    {
        private readonly Sampled<int> _throttle = new Sampled<int>();
        private readonly Sampled<int> _steer = new Sampled<int>();
        //private readonly Timer _timer = new Timer(25);
        private int _running;
        private bool _isUpdating;
        private readonly LctpClient _client;

        public Switch HeadlightSwitch { get; } = new Switch();
        public Switch LeftBlinkerSwitch { get; } = new Switch();
        public Switch RightBlinkerSwitch { get; } = new Switch();
        public bool Connected => _client.Connected;
        private LegoCarState _state = new LegoCarState();

        public LegoCarClient(string host, int port)
        {
            _client = new LctpClient(host, port);
            //_timer.Elapsed += UpdateAsync;
        }

        public async Task UpdateAsync()
        {
            if (Interlocked.CompareExchange(ref _running, 1, 0) != 0)
            {
                return;
            }
            try
            {
                _isUpdating = true;
                if (!await DoUpdate())
                {
                    await _client.PingAsync();
                }
            }
            finally
            {
                _isUpdating = false;
                Interlocked.Exchange(ref _running, 0);
            }
        }

        private int _speed;
        public void SetMotorSpeed(int speed)
        {
            _speed = speed;
        }


        private int _steerAngle;
        public void SetSteer(int steer)
        {
            _steerAngle = steer;
        }

        private async Task<bool> DoUpdate()
        {
            var updated = false;

            _throttle.Value = _speed;
            if (_throttle.HasChanged)
            {
                await _client.SetAsync("motor/speed", $"{_throttle.Value}");
            }

            _steer.Value = _steerAngle;
            if (_steer.HasChanged)
            {
                await _client.SetAsync("steer", $"{_steer.Value}");
            }

            if (HeadlightSwitch.HasChanged())
            {
                await _client.SetAsync("headlights", "toggle");
                HeadlightSwitch.UpdateWasOn();
                updated = true;
            }
            if (LeftBlinkerSwitch.HasChanged())
            {
                await _client.SetAsync("blinker/left", "toggle");
                LeftBlinkerSwitch.UpdateWasOn();
                updated = true;
            }
            if (RightBlinkerSwitch.HasChanged())
            {
                await _client.SetAsync("blinker/right", "toggle");
                RightBlinkerSwitch.UpdateWasOn();
                updated = true;
            }

            var stateResult = await _client.GetAsync("state");
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
        
        public void Connect()
        {
            _client.Connect();
            //_timer.Start();
        }

        public async Task DisconnectAsync()
        {
            //_timer.Stop();
            
            Console.WriteLine("Waiting for update to finish");
            while (_isUpdating)
            {
                await Task.Delay(10);
            }
            Console.WriteLine("Disconnecting");
            _client.Disconnect();
        }
        
        public void Dispose()
        {
            //_timer.Dispose();
            _client?.Dispose();
        }

        public LegoCarState GetState() => _state;

        public Double3 GetEulerAngles() => _state.EulerAngles;

        public Quatd GetQuaternion() => _state.Quaternion;
    }
}