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
                
            };
            
            _connectButton = new NSButton
                {
                    Title = "Connect",
                };

            View = new NSView()
            .WithSubview(_hostField, (c, p) => new[]
            {
                c.CenterXAnchor.ConstraintEqualToAnchor(p.CenterXAnchor),
                c.CenterYAnchor.ConstraintEqualToAnchor(p.CenterYAnchor)
            })
            .WithSubview(_connectButton, (c, p) => new []
            {
                c.TopAnchor.ConstraintEqualToAnchor(_hostField.BottomAnchor)
            });
        }
    }
}