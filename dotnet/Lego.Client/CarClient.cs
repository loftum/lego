using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using LCTP.Core.Client;
using Maths;
using Timer = System.Timers.Timer;

namespace Lego.Client
{
    public class CarClient : IRotationProvider, IDisposable
    {
        private readonly Sampled<int> _throttle = new Sampled<int>();
        private readonly Sampled<int> _steer = new Sampled<int>();
        private readonly Timer _timer = new Timer(25);
        private int _updating = 0;
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
            _timer.Elapsed += Update;
        }

        private async void Update(object sender, ElapsedEventArgs e)
        {
            if (Interlocked.CompareExchange(ref _updating, 1, 0) != 0)
            {
                return;
            }
            
            try
            {
                var sw = new Stopwatch();
                sw.Start();
                if (!await DoUpdate())
                {
                    await _client.Ping();
                }
                sw.Stop();
                Console.WriteLine($"Elapsed: {sw.Elapsed} ({sw.ElapsedMilliseconds} ms)");
            }
            finally
            {
                Interlocked.Exchange(ref _updating, 0);
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

            var rotationResult = await _client.Get("orientation");
            if (rotationResult.StatusCode == 200 && Vector3.TryParse(rotationResult.Content, out var rotation))
            {
                Console.WriteLine($"Rotation: {rotation}");
                _rotation = new Vector3(rotation.X, rotation.Y, rotation.Z);
                updated = true;
            }
            else
            {
                Console.WriteLine($"Bad rotation status code: {rotationResult.StatusCode}");
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

        public Vector3 GetRotation() => _rotation;
    }
}