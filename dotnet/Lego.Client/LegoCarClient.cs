using System;
using System.Threading;
using System.Threading.Tasks;
using LCTP.Core.Client;
using Lego.Core;
using Maths;

namespace Lego.Client
{
    public class LegoCarClient : IRotationProvider, ILegoCarStateProvider, IDisposable
    {
        private readonly Sampled<int> _throttle = new Sampled<int>();
        private readonly Sampled<int> _steer = new Sampled<int>();
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
        
        public async Task UpdateAsync()
        {
            try
            {
                _isUpdating = true;
                if (!await DoUpdateAsync())
                {
                    await _client.PingAsync();
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        

        private async Task<bool> DoUpdateAsync()
        {
            _throttle.Value = _speed;
            _steer.Value = _steerAngle;

            var request = _throttle.HasChanged || _steer.HasChanged
                ? _client.SetAsync("input", new LegoCarInput
                {
                    Throttle = _speed,
                    SteerAngle = _steerAngle
                }.Serialize())
                : _client.GetAsync("state");
            
            
            var response = await request;
        
            if (response.StatusCode == 200)
            {
                if (LegoCarState.TryParse(response.Content, out var state))
                {
                    _state = state;    
                }
                else
                {
                    Console.WriteLine($"Bad state string: {response.Content}");
                }
            }
            else
            {
                Console.WriteLine($"Bad state status code: {response.StatusCode}");
            }

            if (HeadlightSwitch.HasChanged())
            {
                await _client.SetAsync("headlights", "toggle");
                HeadlightSwitch.UpdateWasOn();
            }
            if (LeftBlinkerSwitch.HasChanged())
            {
                await _client.SetAsync("blinker/left", "toggle");
                LeftBlinkerSwitch.UpdateWasOn();
            }
            if (RightBlinkerSwitch.HasChanged())
            {
                await _client.SetAsync("blinker/right", "toggle");
                RightBlinkerSwitch.UpdateWasOn();
            }
            
            return true;
        }
        
        public void Connect()
        {
            _client.Connect();
        }

        public async Task DisconnectAsync()
        {
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
            _client?.Dispose();
        }

        public LegoCarState GetState() => _state;

        public Double3 GetEulerAngles() => _state.EulerAngles;

        public Quatd GetQuaternion() => _state.Quaternion;
    }
}