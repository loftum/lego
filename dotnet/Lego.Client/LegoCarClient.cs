using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LCTP.Core;
using LCTP.Core.Client;
using Lego.Core;
using Lego.Core.Description;
using Maths;

namespace Lego.Client
{
    public class LegoCarClient : IRotationProvider, ILegoCarStateProvider, IDisposable
    {
        private readonly Sampled<int> _throttle = new Sampled<int>();
        private readonly Sampled<int> _steer = new Sampled<int>();
        private bool _isUpdating;
        private readonly ILctpClient _client;

        public Switch HeadlightSwitch { get; } = new Switch();
        public Switch LeftBlinkerSwitch { get; } = new Switch();
        public Switch RightBlinkerSwitch { get; } = new Switch();
        public bool Connected => _client.Connected;
        private LegoCarState _state = new LegoCarState();
        private bool _isDisconnecting;

        public LegoCarClient(ILctpClient client)
        {
            _client = client;
            _client.OnResponseReceived = OnResponseReceived;
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
            if (_isDisconnecting)
            {
                return;
            }
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
            
            
            await request;

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
        
        private Task OnResponseReceived(ResponseMessage response)
        {
            if (LegoCarState.TryParse(response.Content, out var state))
            {
                _state = state;
            }
            return Task.CompletedTask;
        }
        
        public Task ConnectAsync()
        {
            return _client.ConnectAsync();
        }

        public async Task DisconnectAsync()
        {
            _isDisconnecting = true;
            Console.WriteLine("Waiting for update to finish");
            while (_isUpdating)
            {
                await Task.Delay(10);
            }
            Console.WriteLine("Disconnecting");
            await _client.DisconnectAsync();
        }
        
        public void Dispose()
        {
            _client?.Dispose();
        }

        public LegoCarState GetState() => _state;
        public LegoCarDescriptor GetCarDescriptor() => LegoCarDescriptor.RaceCar();

        public Double3 GetEulerAngles() => _state.EulerAngles;

        public Quatd GetQuaternion() => _state.Quaternion;
    }
}