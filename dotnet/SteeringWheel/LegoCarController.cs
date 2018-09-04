using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using CoreMotion;
using LCTP.Client;

namespace SteeringWheel
{
    public static class MathExtensions
    {
        public static int ToDeg(this double rad)
        {
            return (int) (rad * 180 / Math.PI);
        }
    }

    public class LegoCarController
    {
        private CancellationTokenSource _source;
        private Task _task;
        private LctpClient _client;
        private readonly CMMotionManager _motionManager = MotionManager.Instance;

        public bool Connected => _client?.Connected == true;

        public LegoCarController()
        {

        }

        public void Connect(string host, int port)
        {
            if (Connected)
            {
                return;
            }
            _client = new LctpClient(host, port);
            _client.Connect();
            _motionManager.DeviceMotionUpdateInterval = 0.1;
            _motionManager.StartDeviceMotionUpdates();

            _source = new CancellationTokenSource();
            _task = Update(_source.Token);
        }

        private async Task Update(CancellationToken cancellationToken)
        {
            var sw = new Stopwatch();
            while (true)
            {
                sw.Start();
                await DoUpdate();
                while(sw.ElapsedMilliseconds < 100)
                {
                    await Task.Delay(1);
                }
                sw.Reset();
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        private async Task DoUpdate()
        {
            var attitude = _motionManager.DeviceMotion?.Attitude;
            if (attitude == null)
            {
                return;
            }
            var angle = 90 - _motionManager.DeviceMotion.Attitude.Pitch.ToDeg();
            await _client.Set("steer/angle", $"{angle}");
        }

        internal void Disconnect()
        {
            if (!Connected)
            {
                return;
            }
            _source.Cancel();
            _client.Disconnect();
            _motionManager.StopDeviceMotionUpdates();
            _task = null;
        }
    }
}

