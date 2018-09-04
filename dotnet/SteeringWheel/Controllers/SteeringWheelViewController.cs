using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CoreGraphics;
using CoreMotion;
using LCTP.Client;
using UIKit;

namespace SteeringWheel.Controllers
{
    public partial class SteeringWheelViewController : UIViewController
    {
        private CancellationTokenSource _source;
        private Task _task;
        private LctpClient _client;
        private readonly CMMotionManager _motionManager = MotionManager.Instance;

        private int _angle = 90;
        private int _throttle = 0;

        public SteeringWheelViewController(string host,  int port) : base("SteeringWheelViewController", null)
        {
            _client = new LctpClient(host, port);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            CGAffineTransform trans = CGAffineTransform.MakeRotation((nfloat)(Math.PI * 1.5));
            Throttle.Transform = trans;
            Throttle.MinValue = -127;
            Throttle.MaxValue = 127;

            _client.Connect();
            _source = new CancellationTokenSource();
            _motionManager.DeviceMotionUpdateInterval = 0.1;
            _motionManager.StartDeviceMotionUpdates();
            _task = Update(_source.Token);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
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
                sw.Start();
                await DoUpdate();
                while (sw.ElapsedMilliseconds < 50)
                {
                    await Task.Delay(1);
                }
                sw.Reset();
            }
        }

        partial void Throttle_Cancel(UISlider sender)
        {
            Throttle.Value = 0;
        }

        partial void DisconnectButton_TouchUpInside(UIButton sender)
        {
            Disconnect();
            this.DismissViewController(true, null);
        }

        private async Task DoUpdate()
        {
            var attitude = _motionManager.DeviceMotion?.Attitude;
            if (attitude == null)
            {
                return;
            }
            var angle = 90 - _motionManager.DeviceMotion.Attitude.Pitch.ToDeg();
            if (angle != _angle)
            {
                await _client.Set("steer/angle", $"{angle}");
                _angle = angle;
            }
            var throttle = (int)Throttle.Value;
            if (throttle != _throttle)
            {
                await _client.Set("motor/speed", $"{throttle}");
                _throttle = throttle;
            }
        }

        private void Disconnect()
        {
            if (!_client.Connected)
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

