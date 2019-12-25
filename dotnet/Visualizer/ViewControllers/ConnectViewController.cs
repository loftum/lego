using System;
using AppKit;
using Foundation;
using LCTP.Core.Server;

namespace Visualizer.ViewControllers
{
    public class ConnectViewController : NSViewController
    {
        private readonly NSTextField _hostField;
        private readonly NSTextField _errorField;
        private readonly NSButton _connectButton;
        private readonly NSUserDefaults _userDefaults = new NSUserDefaults("app", NSUserDefaultsType.SuiteName);
        
        public ConnectViewController()
        {
            _hostField = new NSTextField
            {
                StringValue = _userDefaults.StringForKey("host") ?? "host",
                Cell =
                {
                    TextColor = NSColor.DarkGray,
                },
                Bordered = true,
            };

            _connectButton = new NSButton
                {
                    Title = "Connect",
                    Cell =
                    {
                        Bezeled = true
                    }
                };
            _connectButton.Activated += ConnectButtonClicked;

            _errorField = new NSTextField
            {
                TextColor = NSColor.Red,
                Editable = false,
                Bordered = false
            };
            
            View = new NSView()
            .WithSubview(_hostField, (c, p) => new[]
            {
                c.TrailingAnchor.ConstraintEqualToAnchor(p.CenterXAnchor, -10),
                c.CenterYAnchor.ConstraintEqualToAnchor(p.CenterYAnchor),
                c.WidthAnchor.ConstraintGreaterThanOrEqualToConstant(200)
            })
            .WithSubview(_connectButton, (c, p) => new []
            {
                c.LeadingAnchor.ConstraintEqualToAnchor(p.CenterXAnchor, 10),
                c.CenterYAnchor.ConstraintEqualToAnchor(p.CenterYAnchor)
            })
            .WithSubview(_errorField, (c, p) => new []
            {
                c.TopAnchor.ConstraintEqualToAnchor(p.CenterYAnchor),
                c.WidthAnchor.ConstraintGreaterThanOrEqualToConstant(400)
            });
        }

        private void ConnectButtonClicked(object sender, EventArgs e)
        {
            _errorField.StringValue = "";
            if (string.IsNullOrWhiteSpace(_hostField.StringValue))
            {
                return;
            }
            var parts = _hostField.StringValue.Split(':');
            var host = parts[0];
            var port = parts.Length > 1 && int.TryParse(parts[1], out var v) ? v : LctpServer.DefaultPort;
            try
            {
                //PresentViewController(new ViewController());
                _userDefaults.SetString(_hostField.StringValue, "host");
            }
            catch (Exception ex)
            {
                _errorField.StringValue = ex.ToString();
            }
        }
    }
}