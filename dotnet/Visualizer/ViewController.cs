using System;
using AppKit;
using CoreGraphics;
using Foundation;
using Metal;
using MetalKit;
using ObjCRuntime;

namespace Visualizer
{
    public class ViewController: NSViewController
    {
        private readonly MTKView mtkView;
        private readonly Renderer renderer;

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
            renderer = new Renderer(mtkView);
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
            
            var slider = new NSSlider();
            slider.Action = new Selector("Hest");
            view.AddSubview(slider);
            slider.TranslatesAutoresizingMaskIntoConstraints = false;
            slider.MinValue = 0;
            slider.MaxValue = 2 * Math.PI;
            NSNotificationCenter.DefaultCenter.AddObserver(null, Notified, slider);
            
            
            NSLayoutConstraint.ActivateConstraints(new []
            {
                slider.LeadingAnchor.ConstraintEqualToAnchor(view.LeadingAnchor),
                slider.TrailingAnchor.ConstraintEqualToAnchor(view.TrailingAnchor),
                slider.BottomAnchor.ConstraintEqualToAnchor(view.BottomAnchor)
            });


            View = view;
        }

        [Export("Hest", Selector = "Hest")]
        private void Hest()
        {
            Console.WriteLine("Hest");
        }

        private void Notified(NSNotification obj)
        {
            Console.WriteLine($"Notified: {obj.Name}");
        }
    }
}
