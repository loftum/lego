using System;
using System.Threading.Tasks;
using CoreFoundation;
using CoreGraphics;
using CoreMotion;
using Lego.Client;
using Lego.Core;
using Maths;
using UIKit;

namespace SteeringWheel.Controllers
{
    public class SteeringWheelViewController : UIViewController
    {
        private readonly LegoCarClient _client = new LegoCarClient(UIDevice.CurrentDevice.Name);
        private readonly CMMotionManager _motionManager = MotionManager.Instance;

        private readonly UISlider _throttleSlider;
        private readonly UIButton _headlightsButton;
        private readonly UIButton _leftBlinkerButton;
        private readonly UIButton _rightBlinkerButton;
        private readonly UIButton _disconnectButton;
        private readonly UITextField _speedometer;
        private readonly UIImpactFeedbackGenerator _impactFeedback = new UIImpactFeedbackGenerator(UIImpactFeedbackStyle.Heavy);

        public SteeringWheelViewController() : base(null, null)
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
                //v.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor),
                v.TopAnchor.ConstraintEqualTo(View.TopAnchor,10)
            })
            .WithTouchUpInside(DisconnectButton_TouchUpInside);
            
            _speedometer = new UITextField()
                .With(l =>
                {
                    l.BackgroundColor = UIColor.SystemPinkColor;
                    l.Text = "000";
                    l.TextAlignment = UITextAlignment.Center;
                    l.Font = UIFont.PreferredTitle1.WithSize(50f);
                    l.TextColor = UIColor.Orange;
                })
                .WithParent(View)
                .WithConstraints(v => new[]
                {
                    v.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor),
                    v.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor),
                    //v.BottomAnchor.ConstraintEqualTo(_disconnectButton.BottomAnchor)
                });


            // Set throttle positions
            var frame = View.GetFrame();
            const int width = 50;
            const int height = 200;
            _throttleSlider.Frame = new CGRect(frame.Width - 100, frame.Height / 2 - height / 2, width, height);
            _client.WillUpdate = ClientWillUpdate;
            _client.DidUpdate = ClientDidUpdate;
            _client.Disconnected = c => DisconnectAndDismissAsync();

            if (!_motionManager.AccelerometerAvailable)
            {
                throw new Exception("Accelerometer not available");
            }

            if (!_motionManager.GyroAvailable)
            {
                throw new Exception("Gyro not available");
            }

            if (!_motionManager.DeviceMotionAvailable)
            {
                throw new Exception("DeviceMotion not available");
            }

            _motionManager.AccelerometerUpdateInterval = .1;
        }

        private Task ClientDidUpdate(LegoCarClient client, LegoCarState state)
        {
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                _speedometer.Text = $"{state.Motion}";
                var absX = Math.Abs(state.Motion.X);
                var absY = Math.Abs(state.Motion.Y);
                if (absY > 100 && absX > 100 && absX > absY)
                {
                    _impactFeedback.ImpactOccurred();
                }
            });
            return Task.CompletedTask;
        }

        private Task ClientWillUpdate(LegoCarClient client)
        {
            DispatchQueue.MainQueue.DispatchSync(() =>
            {
                _client.SetMotorSpeed((int)_throttleSlider.Value);
                _client.SetSteer(GetSteerAngle());
            });
            return Task.CompletedTask;
        }

        public async Task ConnectAsync(string host, int port)
        {
            await _client.ConnectAsync(host, port);
            
            Console.WriteLine("Starting devicemotion updates");
            _motionManager.StartDeviceMotionUpdates();
        }

        private int GetSteerAngle()
        {
            var deviceMotion = _motionManager.DeviceMotion;
            if (deviceMotion == null)
            {
                Console.WriteLine($"deviceMotion is null _motionManager.DeviceMotionActive={_motionManager.DeviceMotionActive}, _motionManager.DeviceMotionAvailable={_motionManager.DeviceMotionAvailable}");
                return 0;
            }
            var attitude = deviceMotion.Attitude;
            if (attitude == null)
            {
                Console.WriteLine($"attitude is null _motionManager.DeviceMotionActive={_motionManager.DeviceMotionActive}, _motionManager.DeviceMotionAvailable={_motionManager.DeviceMotionAvailable}");
                return 0;
            }
            var angle = 90 - attitude.Pitch.ToDeg();
            Console.WriteLine($"Angle: {angle}");
            return angle;
        }

        private void HeadlightsButton_TouchUpInside(object sender, EventArgs e)
        {
            _client.HeadlightSwitch.Toggle();
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
            await DisconnectAndDismissAsync();
        }
        
        private async Task DisconnectAndDismissAsync()
        {
            await DisconnectAsync();
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                DismissViewController(true, () => { });    
            });
        }

        private async Task DisconnectAsync()
        {
            await _client.DisconnectAsync();
            _motionManager.StopDeviceMotionUpdates();
        }
    }
}

