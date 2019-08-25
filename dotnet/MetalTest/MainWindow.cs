using AppKit;
using CoreGraphics;

namespace MetalTest
{
    public class MainWindow : NSWindow
    {
        public MainWindow(CGRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation)
            : base(contentRect, aStyle, bufferingType, deferCreation)
        {
            Title = "MetalTest";
        }
    }
}
