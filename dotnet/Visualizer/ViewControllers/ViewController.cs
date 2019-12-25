using System;
using AppKit;
using CoreGraphics;
using Devices;
using Lego.Car;
using Metal;
using MetalKit;
using Visualizer.Rendering;

namespace Visualizer.ViewControllers
{
    public class ViewController: NSViewController, IRotationProvider
    {
        private readonly MTKView _mtkView;
        private readonly Renderer _renderer;
        private readonly NSSlider _xSlider;
        private readonly NSSlider _ySlider;

        public ViewController()
        {
            var device = MTLDevice.SystemDefault;
            if (device == null)
            {
                throw new Exception("Metal is not supported on this device");
            }

            _mtkView = new MTKView(new CGRect(), device)
            {
                ColorPixelFormat = MTLPixelFormat.BGRA8Unorm,
                DepthStencilPixelFormat = MTLPixelFormat.Depth32Float
            };
            _renderer = new Renderer(_mtkView, this);
            _mtkView.Delegate = _renderer;

            _xSlider = new NSSlider
            {
                IsVertical = 1,
                MinValue = 0,
                MaxValue = 2 * Math.PI
            };

            _ySlider = new NSSlider
            {
                MinValue = 0,
                MaxValue = 2 * Math.PI
            };
            
            View = new NSView()
                .WithSubview(_mtkView, (c, p) => new []
                {
                    c.LeadingAnchor.ConstraintEqualToAnchor(p.LeadingAnchor),
                    c.TopAnchor.ConstraintEqualToAnchor(p.TopAnchor),
                    c.TrailingAnchor.ConstraintEqualToAnchor(p.TrailingAnchor),
                    c.BottomAnchor.ConstraintEqualToAnchor(p.BottomAnchor)
                })
                .WithSubview(_xSlider, (c, p) => new[]
                {
                    c.TopAnchor.ConstraintEqualToAnchor(p.TopAnchor, 40),
                    c.TrailingAnchor.ConstraintEqualToAnchor(p.TrailingAnchor, -10),
                    c.BottomAnchor.ConstraintEqualToAnchor(p.BottomAnchor, -40)
                })
                .WithSubview(_ySlider, (c, p) => new[]
                {
                    c.LeadingAnchor.ConstraintEqualToAnchor(p.LeadingAnchor, 40),
                    c.TrailingAnchor.ConstraintEqualToAnchor(p.TrailingAnchor, -40),
                    c.BottomAnchor.ConstraintEqualToAnchor(p.BottomAnchor, -10)
                });
        }

        public Vector3 GetRotation()
        {
            return new Vector3(_xSlider.FloatValue, -_ySlider.FloatValue, 0);
        }
    }
}
