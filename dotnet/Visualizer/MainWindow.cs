using System;
using Foundation;
using AppKit;
using CoreGraphics;

namespace Visualizer
{
    [Register("MainWindow")]
    public class MainWindow : NSWindow
    {
        public MainWindow(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        [Export("initWithCoder:")]
        public MainWindow(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        public MainWindow(CGRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation)
            : base(contentRect, aStyle, bufferingType, deferCreation)
        {
            Initialize();
        }

        void Initialize()
        {
            Title = "Viaualizer";
        }
    }
}
