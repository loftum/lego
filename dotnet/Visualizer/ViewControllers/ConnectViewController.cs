using System;
using AppKit;
using Foundation;

namespace Visualizer.ViewControllers
{
    public class ConnectViewController : NSViewController
    {
        private readonly NSTextField _hostField;
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
            });
        }

        private void ConnectButtonClicked(object sender, EventArgs e)
        {
            Console.WriteLine($"Connect to: {_hostField.StringValue}");
        }
    }
}