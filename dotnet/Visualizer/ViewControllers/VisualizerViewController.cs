using System;
using AppKit;
using CoreGraphics;
using Lego.Car;
using Metal;
using MetalKit;
using Visualizer.Rendering;

namespace Visualizer.ViewControllers
{
    public class VisualizerViewController : NSViewController, ICarInput
    {
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
            return 0;
        }

        public int GetSteerAngleDeg()
        {
            return 0;
        }
    }
}