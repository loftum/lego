using System;
using AppKit;
using CoreGraphics;
using Metal;
using MetalKit;
using Visualizer.Rendering.Test;

namespace Visualizer.ViewControllers
{
    public class MetalTestViewController : NSViewController
    {
        private readonly MTKView _mtkView;
        private readonly IMTKViewDelegate _renderer;

        public MetalTestViewController()
        {
            var device = MTLDevice.SystemDefault;
            if (device == null)
            {
                throw new Exception("Metal is not supported on this device");
            }

            _mtkView = new MTKView(new CGRect(), device)
            {
                ColorPixelFormat = MTLPixelFormat.BGRA8Unorm_sRGB,
                DepthStencilPixelFormat = MTLPixelFormat.Depth32Float
            };
            _renderer = new TestRenderer(_mtkView);
            _mtkView.Delegate = _renderer;

            View = new NSView()
                .WithSubview(_mtkView, (c, p) => new[]{
                    c.LeadingAnchor.ConstraintEqualToAnchor(p.LeadingAnchor),
                    c.TopAnchor.ConstraintEqualToAnchor(p.TopAnchor),
                    c.TrailingAnchor.ConstraintEqualToAnchor(p.TrailingAnchor),
                    c.BottomAnchor.ConstraintEqualToAnchor(p.BottomAnchor)
                });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _renderer.Dispose();
                _mtkView.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
