using System;
using AppKit;
using CoreGraphics;
using Lego.Client;
using Metal;
using MetalKit;
using Visualizer.Rendering;

namespace Visualizer.ViewControllers
{
    public class VisualizerViewController : NSViewController, ICarInput
    {
        private readonly NSSlider _throttleSlider;
        private readonly NSSlider _steerSlider;
        
        private readonly CarClient _client;
        private readonly Renderer _renderer;
        private readonly MTKView _mtkView;

        public VisualizerViewController(string host, int port)
        {
            _client = new CarClient(host, port, this);
            _client.Connect();

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
            _renderer = new Renderer(_mtkView, _client);
            _mtkView.Delegate = _renderer;
            
            _throttleSlider = new NSSlider
            {
                IsVertical = 1,
                MinValue = -255,
                MaxValue = 255,
                IntValue = 0
            };

            _steerSlider = new NSSlider
            {
                MinValue = -90,
                MaxValue = 90,
                IntValue = 0
            };
            
            View = new NSView()
                .WithSubview(_mtkView, (c, p) => new []
                {
                    c.LeadingAnchor.ConstraintEqualToAnchor(p.LeadingAnchor),
                    c.TopAnchor.ConstraintEqualToAnchor(p.TopAnchor),
                    c.TrailingAnchor.ConstraintEqualToAnchor(p.TrailingAnchor),
                    c.BottomAnchor.ConstraintEqualToAnchor(p.BottomAnchor)
                })
                .WithSubview(_throttleSlider, (c, p) => new[]
                {
                    c.TopAnchor.ConstraintEqualToAnchor(p.TopAnchor, 40),
                    c.TrailingAnchor.ConstraintEqualToAnchor(p.TrailingAnchor, -10),
                    c.BottomAnchor.ConstraintEqualToAnchor(p.BottomAnchor, -40)
                })
                .WithSubview(_steerSlider, (c, p) => new[]
                {
                    c.LeadingAnchor.ConstraintEqualToAnchor(p.LeadingAnchor, 40),
                    c.TrailingAnchor.ConstraintEqualToAnchor(p.TrailingAnchor, -40),
                    c.BottomAnchor.ConstraintEqualToAnchor(p.BottomAnchor, -10)
                });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client.Disconnect();
                _client.Dispose();
                _renderer.Dispose();
                _mtkView.Dispose();
            }
            base.Dispose(disposing);
        }

        public int GetThrottle()
        {
            return _throttleSlider.IntValue;
        }

        public int GetSteerAngleDeg()
        {
            return _steerSlider.IntValue;
        }
    }
}