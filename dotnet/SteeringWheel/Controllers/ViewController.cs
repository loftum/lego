using System;
using Foundation;
using LCTP.Core.Server;
using UIKit;

namespace SteeringWheel.Controllers
{
    public class ViewController : UIViewController
    {
        private readonly UITextField HostField;
        private readonly UIButton ConnectButton;
        private readonly NSUserDefaults _userDefaults = new NSUserDefaults("app", NSUserDefaultsType.SuiteName);

        public ViewController()
        {
            View.BackgroundColor = UIColor.White;
            HostField = new UITextField
            {
                BorderStyle = UITextBorderStyle.RoundedRect,
                Text = _userDefaults.StringForKey("host") ?? "host",
                TextColor = UIColor.DarkGray
            }
            .WithParent(View)
            .WithConstraints(v => new[]
            {
                v.TopAnchor.ConstraintEqualTo(View.TopAnchor, 100),
                v.WidthAnchor.ConstraintGreaterThanOrEqualTo(200),
                v.TrailingAnchor.ConstraintEqualTo(View.CenterXAnchor, 20),
            })
            .With(v => v.SpellCheckingType = UITextSpellCheckingType.No);

            ConnectButton = new UIButton()
            .WithParent(View)
            .WithTitle("Connect")
            .WithTitleColor(UIColor.Blue)
            .WithTouchUpInside(ConnectButton_TouchUpInside)
            .WithConstraints(v => new[]{
                v.TopAnchor.ConstraintEqualTo(View.TopAnchor, 100),
                v.LeadingAnchor.ConstraintEqualTo(View.CenterXAnchor, 20),
                v.WidthAnchor.ConstraintGreaterThanOrEqualTo(100)
            });
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
        }

        private void ConnectButton_TouchUpInside(object sender, EventArgs e)
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
                using (var controller = new UIAlertController())
                {
                    using(var action = UIAlertAction.Create(ex.Message, UIAlertActionStyle.Default, null))
                    {
                        controller.AddAction(action);
                        ShowViewController(controller, this);
                    }
                }
            }
        }
    }
}