using System;
using CoreMotion;
using Foundation;
using LCTP.Server;
using UIKit;

namespace SteeringWheel.Controllers
{
    public partial class ViewController : UIViewController
    {
        private readonly NSUserDefaults _userDefaults = new NSUserDefaults("app", NSUserDefaultsType.SuiteName);

        public ViewController() : base("ViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            HostField.Text = _userDefaults.StringForKey("host") ?? "host";
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
        }

        partial void ConnectButton_TouchUpInside(UIButton sender)
        {
            Connect();
        }

        private void Connect()
        {
            if (string.IsNullOrWhiteSpace(HostField.Text))
            {
                return;
            }
            var parts = HostField.Text.Split(':');
            var host = parts[0];
            var port = parts.Length > 1 && int.TryParse(parts[1], out var v) ? v : LctpServer.DefaultPort;
            try
            {
                ShowViewController(new SteeringWheelViewController(host, port), this);
                _userDefaults.SetString(HostField.Text, "host");
            }
            catch (Exception ex)
            {
                var controller = new UIAlertController();
                controller.AddAction(UIAlertAction.Create(ex.Message, UIAlertActionStyle.Default, null));
                ShowViewController(controller, this);
            }
        }
    }
}

