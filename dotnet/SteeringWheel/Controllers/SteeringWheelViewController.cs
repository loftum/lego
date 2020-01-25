using System;
using System.Threading.Tasks;
using CoreFoundation;
using CoreGraphics;
using CoreMotion;
using Lego.Client;
using Maths;
using UIKit;

namespace SteeringWheel.Controllers
{
    public class SteeringWheelViewController : UIViewController, ICarInput
    {
        private readonly LegoCarClient _client;
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

            _client = new LegoCarClient(host, port, this);
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

        private async void DisconnectButton_TouchUpInside(object sender, EventArgs e)
        {
            await DisconnectAsync();
            DismissViewController(true, () => { });
        }

        private async Task DisconnectAsync()
        {
            if (!_client.Connected)
            {
                return;
            }
            await _client.DisconnectAsync();
            _motionManager.StopDeviceMotionUpdates();
        }

        public Task<int> GetThrottleAsync() => DispatchQueue.MainQueue.DispatchAsync(() => (int)_throttleSlider.Value);

        public Task<int> GetSteerAngleDegAsync()
        {
            return DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                var attitude = _motionManager.DeviceMotion?.Attitude;
                var angle = attitude == null ? 0 : 90 - attitude.Pitch.ToDeg();
                Console.WriteLine($"Angle: {angle}");
                return angle;
            });
        }
    }
    
    public static class DispatchQueueExtensions
    {
        public static Task<T> DispatchAsync<T>(this DispatchQueue queue, Func<T> getValue)
        {
            var tcs = new TaskCompletionSource<T>();
            queue.DispatchSync(() =>
            {
                tcs.SetResult(getValue());
            });
            return tcs.Task;
        }
    }
}

