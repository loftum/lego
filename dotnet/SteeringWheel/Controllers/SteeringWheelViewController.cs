using System;
using CoreGraphics;
using CoreMotion;
using Lego.Car;
using UIKit;

namespace SteeringWheel.Controllers
{
    public class SteeringWheelViewController : UIViewController, ICarInput
    {
        private readonly CarClient _client;
        private readonly CMMotionManager _motionManager = MotionManager.Instance;

        private readonly UISlider _throttleSlider;
        private readonly UIButton _headlightsButton;
        private readonly UIButton _leftBlinkerButton;
        private readonly UIButton _rightBlinkerButton;
        private readonly UIButton _disconnectButton;

        public SteeringWheelViewController(string host, int port) : base(null, null)
        {
            View.BackgroundColor = UIColor.White;   
            
            _throttleSlider = new UISlider
            {
                MinValue = -255,
                MaxValue = 255,
            }
            .WithParent(View)
            .Rotate(Math.PI * 3 / 2)
            .WithTouchUpInside(Throttle_Cancel)
            .WithTouchUpOutside(Throttle_Cancel)
            ;

            _headlightsButton = new UIButton()
            .WithTitle("💡")
            .WithParent(View)
            .WithConstraints(v => new[] {
                v.TopAnchor.ConstraintEqualTo(View.TopAnchor, 100),
                v.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor)
            })
            .WithTouchUpInside(HeadlightsButton_TouchUpInside);

            _leftBlinkerButton = new UIButton()
            .WithTitle("⇦")
            .WithTitleColor(UIColor.Green)
            .WithParent(View)
            .WithConstraints(v => new[] {
                v.TopAnchor.ConstraintEqualTo(View.TopAnchor, 100),
                v.TrailingAnchor.ConstraintEqualTo(_headlightsButton.LeadingAnchor, -50),
            })
            .WithTouchUpInside(LeftBlinkerButton_TouchUpInside);

            _rightBlinkerButton = new UIButton()
            .WithTitle("⇨")
            .WithTitleColor(UIColor.Green)
            .WithParent(View)
            .WithConstraints(v => new[] {
                v.TopAnchor.ConstraintEqualTo(View.TopAnchor, 100),
                v.LeadingAnchor.ConstraintEqualTo(_headlightsButton.TrailingAnchor, 50),
            })
            .WithTouchUpInside(RightBlinkerButton_TouchUpInside);

            _disconnectButton = new UIButton()
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
            _throttleSlider.Frame = new CGRect(frame.Width - 100, frame.Height / 2 - height / 2, width, height);

            _client = new CarClient(host, port, this);
            _client.Connect();
            _motionManager.DeviceMotionUpdateInterval = 0.1;
            _motionManager.StartDeviceMotionUpdates();
        }

        private void HeadlightsButton_TouchUpInside(object sender, EventArgs e)
        {
            _client.HeadlightSwitch.Toggle();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        private void RightBlinkerButton_TouchUpInside(object sender, EventArgs e)
        {
            _client.RightBlinkerSwitch.Toggle();
        }

        private void LeftBlinkerButton_TouchUpInside(object sender, EventArgs e)
        {
            _client.LeftBlinkerSwitch.Toggle();
        }

        private void Throttle_Cancel(object sender, EventArgs e)
        {
            _throttleSlider.Value = 0;
        }

        private void DisconnectButton_TouchUpInside(object sender, EventArgs e)
        {
            Disconnect();
            DismissViewController(true, () => { });
        }

        private void Disconnect()
        {
            if (!_client.Connected)
            {
                return;
            }
            _client.Disconnect();
            _motionManager.StopDeviceMotionUpdates();
        }

        public int GetThrottle() => (int) _throttleSlider.Value;

        public int GetSteerAngleDeg()
        {
            var attitude = _motionManager.DeviceMotion?.Attitude;
            if (attitude == null)
            {
                return 0;
            }
            return 90 - attitude.Pitch.ToDeg();
        }
    }
}

