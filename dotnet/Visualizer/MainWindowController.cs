using System;
using Foundation;
using AppKit;
using CoreGraphics;
using Visualizer.ViewControllers;

namespace Visualizer
{
    [Register("MainWindowController")]
    public class MainWindowController : NSWindowController
    {
        private readonly ViewController _vc = new ViewController();

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
            Window.ContentView = _vc.View;
        }

        public new MainWindow Window => (MainWindow)base.Window;
    }
}
