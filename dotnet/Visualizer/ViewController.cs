using System;
using AppKit;
using CoreGraphics;
using Foundation;
using Metal;
using MetalKit;
using OpenTK;

namespace Visualizer
{
    public interface IRotationProvider
    {
        Vector3 GetRotation();
    }

    public class ViewController: NSViewController, IRotationProvider
    {
        private readonly MTKView mtkView;
        private readonly Renderer renderer;
        private readonly NSSlider _xSlider;
        private readonly NSSlider _ySlider;

        public ViewController()
        {
            Console.WriteLine("ViewController ctor");
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
            renderer = new Renderer(mtkView, this);
            mtkView.Delegate = renderer;

            var view = new NSView();

            view.AddSubview(mtkView);
            mtkView.TranslatesAutoresizingMaskIntoConstraints = false;
            NSLayoutConstraint.ActivateConstraints(new []
            {
                mtkView.LeadingAnchor.ConstraintEqualToAnchor(view.LeadingAnchor),
                mtkView.TopAnchor.ConstraintEqualToAnchor(view.TopAnchor),
                mtkView.TrailingAnchor.ConstraintEqualToAnchor(view.TrailingAnchor),
                mtkView.BottomAnchor.ConstraintEqualToAnchor(view.BottomAnchor)
            });

            var xSlider = new NSSlider
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                IsVertical = 1,
                MinValue = 0,
                MaxValue = 2 * Math.PI
            };
            view.AddSubview(xSlider);
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                xSlider.TopAnchor.ConstraintEqualToAnchor(view.TopAnchor, 40),
                xSlider.TrailingAnchor.ConstraintEqualToAnchor(view.TrailingAnchor, -10),
                xSlider.BottomAnchor.ConstraintEqualToAnchor(view.BottomAnchor, -40)
            });

            var ySlider = new NSSlider
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                MinValue = 0,
                MaxValue = 2 * Math.PI
            };
            view.AddSubview(ySlider);
            
            NSLayoutConstraint.ActivateConstraints(new []
            {
                ySlider.LeadingAnchor.ConstraintEqualToAnchor(view.LeadingAnchor, 40),
                ySlider.TrailingAnchor.ConstraintEqualToAnchor(view.TrailingAnchor, -40),
                ySlider.BottomAnchor.ConstraintEqualToAnchor(view.BottomAnchor, -10)
            });

            _xSlider = xSlider;
            _ySlider = ySlider;
            View = view;
        }

        public Vector3 GetRotation() => new Vector3(_xSlider.FloatValue, -_ySlider.FloatValue, 0);
    }
}
