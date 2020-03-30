using System;
using System.Diagnostics;
using System.Net.Sockets;
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
        private readonly LegoCarClient _client;
        private readonly CMMotionManager _motionManager = MotionManager.Instance;

        private readonly UISlider _throttleSlider;
        private readonly UIButton _headlightsButton;
        private readonly UIButton _leftBlinkerButton;
        private readonly UIButton _rightBlinkerButton;
        private readonly UIButton _disconnectButton;
        private readonly InterlockedAsyncTimer _timer = new InterlockedAsyncTimer(15);

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
            const int width = 50;
            const int height = 200;
            _throttleSlider.Frame = new CGRect(frame.Width - 100, frame.Height / 2 - height / 2, width, height);

            _client = new LegoCarClient(host, port);
            _client.Connect();
            _motionManager.DeviceMotionUpdateInterval = 0.1;
            _motionManager.StartDeviceMotionUpdates();
            _timer.Elapsed = UpdateAsync;
            _timer.Start();
        }

        private async Task UpdateAsync()
        {
            var sw = new Stopwatch();
            sw.Start();
            DispatchQueue.MainQueue.DispatchSync(() =>
            {
                _client.SetMotorSpeed((int)_throttleSlider.Value);
                _client.SetSteer(GetSteerAngle());
                
            });
            try
            {
                await _client.UpdateAsync();
            }
            catch (Exception exception)
            {
                var inner = exception.GetBaseException();
                switch (inner)
                {
                    case null:
                        Console.WriteLine(exception);
                        await DisconnectAndDismissAsync();
                        break;
                    case SocketException socketException:
                        Console.WriteLine($"SocketException: {socketException.SocketErrorCode}");
                        Console.WriteLine(exception);
                        await DisconnectAndDismissAsync();
                        break;
                    default:
                        Console.WriteLine(exception);
                        await DisconnectAndDismissAsync();
                        break;
                }
            }
            finally
            {
                sw.Stop();
                Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}ms");
            }
        }

        private int GetSteerAngle()
        {
            var attitude = _motionManager.DeviceMotion?.Attitude;
            var angle = 90 - attitude?.Pitch.ToDeg() ?? 0;
            //Console.WriteLine($"Angle: {angle}");
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
            _timer.Stop();
            _timer.Elapsed = null;
            await _client.DisconnectAsync();
            _motionManager.StopDeviceMotionUpdates();
        }
    }
}

