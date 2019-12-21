using System;
using AppKit;
using CoreGraphics;
using Foundation;
using Metal;
using MetalKit;
using ObjCRuntime;

namespace Visualizer
{
    public interface IRotationProvider
    {
        float GetRotation();
    }

    public class ViewController: NSViewController, IRotationProvider
    {
        private readonly MTKView mtkView;
        private readonly Renderer renderer;
        private readonly NSSlider _slider;

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

            var slider = new NSSlider
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                MinValue = 0,
                MaxValue = 2 * Math.PI
            };
            view.AddSubview(slider);
            NSNotificationCenter.DefaultCenter.AddObserver(null, Notified, slider);
            
            NSLayoutConstraint.ActivateConstraints(new []
            {
                slider.LeadingAnchor.ConstraintEqualToAnchor(view.LeadingAnchor, 10),
                slider.TrailingAnchor.ConstraintEqualToAnchor(view.TrailingAnchor, -10),
                slider.BottomAnchor.ConstraintEqualToAnchor(view.BottomAnchor, -10)
            });

            _slider = slider;
            View = view;
        }

        public float GetRotation() => _slider.FloatValue;

        private void Notified(NSNotification obj)
        {
            Console.WriteLine($"Notified: {obj.Name}");
        }
    }
}
