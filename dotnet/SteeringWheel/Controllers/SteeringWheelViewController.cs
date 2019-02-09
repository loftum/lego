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
    public class Switch
    {
        public bool IsOn { get; set; }
        public bool WasOn { get; set; }
        public bool HasChanged() => IsOn != WasOn;
        public void UpdateWasOn()
        {
            WasOn = IsOn;
        }
    }

    public partial class SteeringWheelViewController : UIViewController
    {
        private CancellationTokenSource _source;
        private Task _task;
        private readonly LctpClient _client;
        private readonly CMMotionManager _motionManager = MotionManager.Instance;

        private int _angle = 90;
        private int _backSpeed = 0;
        private int _frontSpeed = 0;
        private readonly Switch _frontThrottleSwitch = new Switch();
        private readonly Switch _headlightSwitch = new Switch();
        private readonly Switch _leftBlinkerSwitch = new Switch();
        private readonly Switch _rightBlinkerSwitch = new Switch();

        public SteeringWheelViewController(string host, int port) : base("SteeringWheelViewController", null)
        {
            _client = new LctpClient(host, port);
            _client.Connect();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            CGAffineTransform trans = CGAffineTransform.MakeRotation((nfloat)(Math.PI * 1.5));
            Throttle.Transform = trans;
            Throttle.MinValue = -255;
            Throttle.MaxValue = 255;
            FrontThrottle.Transform = trans;
            FrontThrottle.MinValue = -255;
            FrontThrottle.MaxValue = 255;

            _source = new CancellationTokenSource();
            _motionManager.DeviceMotionUpdateInterval = 0.1;
            _motionManager.StartDeviceMotionUpdates();
            _task = Update(_source.Token);
        }

        partial void HeadlightsButton_TouchUpInside(UIButton sender)
        {
            _headlightSwitch.IsOn = !_headlightSwitch.IsOn;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        partial void RightBlinkerButton_TouchUpInside(UIButton sender)
        {
            _rightBlinkerSwitch.IsOn = !_rightBlinkerSwitch.IsOn;
        }

        partial void LeftBlinkerButton_TouchUpInside(UIButton sender)
        {
            _leftBlinkerSwitch.IsOn = !_leftBlinkerSwitch.IsOn;
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

        partial void Throttle_Cancel(UISlider sender)
        {
            Throttle.Value = 0;
        }

        partial void FrontThrottle_Enable(UISlider sender)
        {
            _frontThrottleSwitch.IsOn = true;
            View.BackgroundColor = UIColor.Cyan;
        }

        partial void FrontThrottle_Cancel(UISlider sender)
        {
            _frontThrottleSwitch.IsOn = false;
            View.BackgroundColor = UIColor.White;
            FrontThrottle.Value = 0;
        }

        partial void DisconnectButton_TouchUpInside(UIButton sender)
        {
            Disconnect();
            this.DismissViewController(true, null);
        }

        private async Task<bool> DoUpdate()
        {
            var updated = false;
            var attitude = _motionManager.DeviceMotion?.Attitude;
            if (attitude == null)
            {
                return updated;
            }
            var angle = 90 - _motionManager.DeviceMotion.Attitude.Pitch.ToDeg();
            if (angle != _angle)
            {
                await _client.Set("steer/angle", $"{angle}");
                _angle = angle;
                updated = true;
            }

            if (_frontThrottleSwitch.IsOn)
            {
                var backThrottle = (int)Throttle.Value;
                if (backThrottle != _backSpeed)
                {
                    await _client.Set("motor/0/speed", $"{backThrottle}");
                    _backSpeed = backThrottle;
                    updated = true;
                }

                var frontThrottle = (int)FrontThrottle.Value;
                if (frontThrottle != _frontSpeed || !_frontThrottleSwitch.WasOn)
                {
                    _frontThrottleSwitch.WasOn = true;
                    await _client.Set("motor/1/speed", $"{frontThrottle}");
                    _frontSpeed = frontThrottle;
                    updated = true;
                }
            }
            else
            {
                var backSpeed = (int)Throttle.Value;
                if (backSpeed != _backSpeed || _frontThrottleSwitch.WasOn)
                {
                    _frontThrottleSwitch.WasOn = false;
                    await _client.Set("motor/speed", $"{backSpeed}");
                    _backSpeed = backSpeed;
                    _frontSpeed = backSpeed;
                    updated = true;
                }
            }
            if (_headlightSwitch.HasChanged())
            {
                await _client.Set("headlights", "toggle");
                _headlightSwitch.UpdateWasOn();
                updated = true;
            }
            if (_leftBlinkerSwitch.HasChanged())
            {
                await _client.Set("blinker/left", "toggle");
                _leftBlinkerSwitch.UpdateWasOn();
                updated = true;
            }
            if (_rightBlinkerSwitch.HasChanged())
            {
                await _client.Set("blinker/right", "toggle");
                _rightBlinkerSwitch.UpdateWasOn();
                updated = true;
            }

            return updated;
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

