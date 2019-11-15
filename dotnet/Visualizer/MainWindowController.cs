using System;
using Foundation;
using AppKit;
using CoreGraphics;

namespace Visualizer
{
    [Register("MainWindowController")]
    public class MainWindowController : NSWindowController
    {
        public MainWindowController(IntPtr handle) : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public MainWindowController(NSCoder coder) : base(coder)
        {
        }

        public MainWindowController() : base("MainWindow")
        {
            CGRect contentRect = new CGRect(0, 0, 1000, 500);
            base.Window = new MainWindow(contentRect, NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable, NSBackingStore.Buffered, false);
        }

        public new MainWindow Window => (MainWindow)base.Window;
    }
}
