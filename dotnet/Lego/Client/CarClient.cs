using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Devices;
using LCTP.Core.Client;

namespace Lego.Car
{
    public class CarClient : IRotationProvider, IDisposable
    {
        private Sampled<int> _throttle;
        private Sampled<int> _steer;
        private readonly CancellationTokenSource _source = new CancellationTokenSource();
        private Task _task;
        private readonly LctpClient _client;
        private readonly ICarInput _input;
        private Vector3 _rotation;

        public Switch HeadlightSwitch { get; } = new Switch();
        public Switch LeftBlinkerSwitch { get; } = new Switch();
        public Switch RightBlinkerSwitch { get; } = new Switch();
        public bool Connected => _client.Connected;

        public CarClient(string host, int port, ICarInput input)
        {
            _input = input;
            _client = new LctpClient(host, port);
        }
        
        private async Task Update(CancellationToken cancellationToken)
        {
            var sw = new Stopwatch();
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                Console.WriteLine("Update");
                sw.Start();
                if (!await DoUpdate())
                {
                    await _client.Ping();
                }
                if (sw.ElapsedMilliseconds < 50)
                {
                    await Task.Delay(50 - (int)sw.ElapsedMilliseconds, cancellationToken);
                }
                sw.Reset();
            }
            Console.WriteLine("Update done");
        }

        private async Task<bool> DoUpdate()
        {
            var updated = false;

            _throttle.Value = _input.GetThrottle();
            if (_throttle.HasChanged)
            {
                await _client.Set("motor/speed", $"{_throttle.Value}");
            }

            _steer.Value = _input.GetSteerAngleDeg();
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

            var rotationResult = await _client.Get("rotation");
            if (rotationResult.StatusCode == 200 && Vector3.TryParse(rotationResult.Content, out var rotation))
            {
                _rotation = rotation;
            }
            return updated;
        }
        
        public void Dispose()
        {
            _source.Cancel();
            _source.Dispose();
            _client?.Dispose();
        }

        public void Connect()
        {
            _client.Connect();
            _task = Update(_source.Token);
        }

        public void Disconnect()
        {
            _source.Cancel();
            _source.Dispose();
            _client.Disconnect();
        }

        public Vector3 GetRotation() => _rotation;
    }
}