using System;
using AppKit;
using CoreGraphics;
using Metal;
using MetalKit;

namespace Visualizer
{
    public class ViewController: NSViewController
    {
        private readonly MTKView mtkView;
        private readonly Renderer renderer;

        public ViewController()
        {
            var device = MTLDevice.SystemDefault;
            if (device == null)
            {
                throw new Exception("Metal is not supported on this device");
            }

            mtkView = new MTKView(new CGRect(), device)
            {
                ColorPixelFormat = MTLPixelFormat.BGRA8Unorm,
                DepthStencilPixelFormat = MTLPixelFormat.Depth32Float
            };
            renderer = new Renderer(mtkView);
            View = mtkView;
        }
    }
}
