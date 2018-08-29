using System;
using LCTP.Client;
using LCTP.Server;
using UIKit;

namespace SteeringWheel
{
    public partial class ViewController : UIViewController
    {
        private LctpClient _client;

        public ViewController(): base("ViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        partial void ConnectButton_TouchUpInside(UIButton sender)
        {
            if (string.IsNullOrWhiteSpace(HostField.Text))
            {
                return;
            }
            var parts = HostField.Text.Split(':');
            var host = parts[0];
            var port = parts.Length > 1 && int.TryParse(parts[1], out var v) ? v : LctpServer.DefaultPort;
            _client = new LctpClient(host, port);
        }
    }
}

