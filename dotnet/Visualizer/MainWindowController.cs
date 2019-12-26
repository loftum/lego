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
        private readonly ConnectViewController _connectViewController = new ConnectViewController();

        public MainWindowController(IntPtr handle) : base(handle)
        {
        }

        [Export("initWithCoder:")]
        public MainWindowController(NSCoder coder) : base(coder)
        {
        }

        public MainWindowController() : base("MainWindow")
        {
            var contentRect = new CGRect(0, 0, 1000, 500);
            base.Window = new MainWindow(contentRect, NSWindowStyle.Titled | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable, NSBackingStore.Buffered, false);
            Window.ContentView = _connectViewController.View;
            _connectViewController.OnConnect += OnConnect;
        }

        private void OnConnect(object sender, ConnectEventArgs e)
        {
            var visualizer = new VisualizerViewController(e.Host, e.Port);
            Window.ContentView = visualizer.View;
        }

        public new MainWindow Window => (MainWindow)base.Window;
    }
}
