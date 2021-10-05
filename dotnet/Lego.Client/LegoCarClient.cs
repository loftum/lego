using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using LCTP.Core;
using LCTP.Core.Client;
using LCTP.Core.Server;
using Lego.Core;
using Lego.Core.Description;
using Maths;

namespace Lego.Client
{
    public class LegoCarClient : IRotationProvider, ILegoCarStateProvider, IDisposable
    {
        private readonly string _name;
        private readonly Sampled<int> _throttle = new Sampled<int>();
        private readonly Sampled<int> _steer = new Sampled<int>();
        private bool _isUpdating;
        private ILctpClient _client;

        public Switch HeadlightSwitch { get; } = new Switch();
        public Switch LeftBlinkerSwitch { get; } = new Switch();
        public Switch RightBlinkerSwitch { get; } = new Switch();
        public bool IsConnected => _client is { Connected: true };
        public Func<LegoCarClient, Task> WillUpdate { get; set; } = _ => Task.CompletedTask;
        public Func<LegoCarClient, LegoCarState, Task> DidUpdate { get; set; } = (_,__) => Task.CompletedTask;
        public Func<LegoCarClient, Task> Disconnected { get; set; } = _ => Task.CompletedTask;
        public Func<LegoCarClient, Task> Connected { get; set; } = _ => Task.CompletedTask;

        private LegoCarState _state = new LegoCarState();
        private bool _isDisconnecting;
        
        private readonly InterlockedAsyncTimer _timer = new InterlockedAsyncTimer(15);

        public LegoCarClient(string name)
        {
            _name = name;
            _timer.Elapsed += TimerElapsed;
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
        
        private async Task TimerElapsed()
        {
            if (_isDisconnecting || !IsConnected)
            {
                return;
            }

            await WillUpdate(this);
            await UpdateAsync();
            await DidUpdate(this, _state);
        }
        
        private async Task UpdateAsync()
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
            catch (Exception exception)
            {
                var inner = exception.GetBaseException();
                switch (inner)
                {
                    case null:
                        Console.WriteLine(exception);
                        break;
                    case SocketException socketException:
                        Console.WriteLine($"SocketException: {socketException.SocketErrorCode}");
                        Console.WriteLine(exception);
                        break;
                    default:
                        Console.WriteLine(exception);
                        break;
                }

                await Disconnected(this);
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

        public Task ConnectAsync(string hostString)
        {
            var parts = hostString.Split(':');
            var host = parts[0];
            var port = parts.Length > 1 && int.TryParse(parts[1], out var v) ? v : LctpServer.DefaultPort;
            return ConnectAsync(host, port);
        }
        
        public async Task ConnectAsync(string host, int port)
        {
            var client = new LctpClient(_name, host, port);
            await client.ConnectAsync();
            client.OnResponseReceived += OnResponseReceived;
            _client = client;
            await Connected(this);
            _timer.Start();
        }

        public async Task DisconnectAsync()
        {
            if (!IsConnected)
            {
                return;
            }
            _isDisconnecting = true;
            _timer.Stop();
            _client.OnResponseReceived -= OnResponseReceived;
            
            Console.WriteLine("Waiting for update to finish");
            while (_isUpdating)
            {
                await Task.Delay(10);
            }
            Console.WriteLine("Disconnecting");
            await _client.DisconnectAsync();
            _client.Dispose();
            _client = null;
            Reset();
            _isDisconnecting = false;
        }

        public void Reset()
        {
            _state = new LegoCarState();
            _throttle.Value = 0;
            _steer.Value = 0;
            HeadlightSwitch.IsOn = false;
            LeftBlinkerSwitch.IsOn = false;
            RightBlinkerSwitch.IsOn = false;
        }
        
        public void Dispose()
        {
            if (_client == null)
            {
                return;
            }

            _client.OnResponseReceived -= OnResponseReceived;
            _client.Dispose();
            _timer.Elapsed -= TimerElapsed;
            _timer.Dispose();
        }

        public LegoCarState GetState() => _state;
        public LegoCarDescriptor GetCarDescriptor() => LegoCarDescriptor.RaceCar();

        public Double3 GetEulerAngles() => _state.EulerAngles;

        public Quatd GetQuaternion() => _state.Quaternion;
    }
}