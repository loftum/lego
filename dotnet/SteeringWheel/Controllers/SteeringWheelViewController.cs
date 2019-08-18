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
    public class UIVerticalSlider : UISlider
    {
        public UIVerticalSlider()
        {
            Transform = CGAffineTransform.MakeRotation((nfloat) (- Math.PI / 2));
        }
    }

    public class SteeringWheelViewController : UIViewController
    {
        private CancellationTokenSource _source = new CancellationTokenSource();
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

        private readonly UISlider Throttle;
        private readonly UISlider FrontThrottle;
        private readonly UIButton HeadlightsButton;
        private readonly UIButton LeftBlinkerButton;
        private readonly UIButton RightBlinkerButton;
        private readonly UIButton DisconnectButton;

        public SteeringWheelViewController(string host, int port) : base(null, null)
        {
            View.BackgroundColor = UIColor.White;   

            FrontThrottle = new UISlider
            {
                MinValue = -255,
                MaxValue = 255
            }
            .WithParent(View)
            .Rotate(Math.PI * 3 / 2)
            .WithTouchDown(FrontThrottle_Enable)
            .WithTouchUpInside(FrontThrottle_Cancel)
            .WithTouchUpOutside(FrontThrottle_Cancel);

            Throttle = new UISlider
            {
                MinValue = -255,
                MaxValue = 255,
            }
            .WithParent(View)
            .Rotate(Math.PI * 3 / 2)
            .WithTouchUpInside(Throttle_Cancel)
            .WithTouchUpOutside(Throttle_Cancel)
            ;

            HeadlightsButton = new UIButton()
            .WithTitle("💡")
            .WithParent(View)
            .WithConstraints(v => new[] {
                v.TopAnchor.ConstraintEqualTo(View.TopAnchor, 100),
                v.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor)
            })
            .WithTouchUpInside(HeadlightsButton_TouchUpInside);

            LeftBlinkerButton = new UIButton()
            .WithTitle("⇦")
            .WithTitleColor(UIColor.Green)
            .WithParent(View)
            .WithConstraints(v => new[] {
                v.TopAnchor.ConstraintEqualTo(View.TopAnchor, 100),
                v.TrailingAnchor.ConstraintEqualTo(HeadlightsButton.LeadingAnchor, -50),
            })
            .WithTouchUpInside(LeftBlinkerButton_TouchUpInside);

            RightBlinkerButton = new UIButton()
            .WithTitle("⇨")
            .WithTitleColor(UIColor.Green)
            .WithParent(View)
            .WithConstraints(v => new[] {
                v.TopAnchor.ConstraintEqualTo(View.TopAnchor, 100),
                v.LeadingAnchor.ConstraintEqualTo(HeadlightsButton.TrailingAnchor, 50),
            })
            .WithTouchUpInside(RightBlinkerButton_TouchUpInside);

            DisconnectButton = new UIButton()
            .WithTitle("Disconnect")
            .WithTitleColor(UIColor.Blue)
            .WithParent(View)
            .WithConstraints(v => new[] {
                v.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                v.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor)
            })
            .WithTouchUpInside(DisconnectButton_TouchUpInside);

            // Set throttle positions
            var frame = View.GetFrame();
            var width = 50;
            var height = 200;
            FrontThrottle.Frame = new CGRect(50, frame.Height / 2 - height / 2, width, height);
            Throttle.Frame = new CGRect(frame.Width - 100, frame.Height / 2 - height / 2, width, height);

            _client = new LctpClient(host, port);
            _client.Connect();
            _motionManager.DeviceMotionUpdateInterval = 0.1;
            _motionManager.StartDeviceMotionUpdates();
            _task = Update(_source.Token);
        }

        private void HeadlightsButton_TouchUpInside(object sender, EventArgs e)
        {
            _headlightSwitch.IsOn = !_headlightSwitch.IsOn;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        private void RightBlinkerButton_TouchUpInside(object sender, EventArgs e)
        {
            _rightBlinkerSwitch.IsOn = !_rightBlinkerSwitch.IsOn;
        }

        private void LeftBlinkerButton_TouchUpInside(object sender, EventArgs e)
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

        private void Throttle_Cancel(object sender, EventArgs e)
        {
            Throttle.Value = 0;
        }

        private void FrontThrottle_Enable(object sender, EventArgs e)
        {
            _frontThrottleSwitch.IsOn = true;
            View.BackgroundColor = UIColor.Cyan;
        }

        private void FrontThrottle_Cancel(object sender, EventArgs e)
        {
            _frontThrottleSwitch.IsOn = false;
            View.BackgroundColor = UIColor.White;
            FrontThrottle.Value = 0;
        }
        private void DisconnectButton_TouchUpInside(object sender, EventArgs e)
        {
            Disconnect();
            DismissViewController(true, () => { });
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
                    Console.WriteLine($"Throttle: {backThrottle}");
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

