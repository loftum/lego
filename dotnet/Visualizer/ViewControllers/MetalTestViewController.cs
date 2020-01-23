using System;
using AppKit;
using CoreGraphics;
using Lego.Client;
using Maths;
using Metal;
using MetalKit;
using Visualizer.Rendering.Car;

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
            _renderer = new CarRenderer(_mtkView, new TestRotationProvider());
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

    public class TestRotationProvider : IRotationProvider
    {
        private double _angle;

        private double NextAngle()
        {
            _angle += (float) (2 * Math.PI / 1000);
            if (_angle >= 2 * Math.PI)
            {
                _angle = 0;
            }

            return _angle;
        }
        
        public Double3 GetEulerAngles()
        {
            var angle = NextAngle();
            return new Double3(0, angle, 0);
        }

        public Quatd GetQuaternion()
        {
            var angle = NextAngle();
            // Camera on top of car
            var wiggle = Math.PI / 12 * Math.Sin(angle * 12);
            return new Quatd(-Math.PI / 2 + wiggle, 0, Math.PI / 2 + angle);
        }
    }
}
