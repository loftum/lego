using AppKit;
using Foundation;

namespace Visualizer
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        private readonly MainWindowController _controller = new MainWindowController();

        public AppDelegate()
        {
            
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            _controller.Window.MakeKeyAndOrderFront(this);
            // Insert code here to initialize your application
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}
