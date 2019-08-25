using System;
using AppKit;
using CoreGraphics;
using Metal;
using MetalKit;

namespace MetalTest
{
    public class ViewController : NSViewController
    {
        private readonly MTKView _view;
        private readonly Renderer _renderer;

        public ViewController()
        {
            var device = MTLDevice.SystemDefault;
            if (device == null)
            {
                throw new Exception("Metal is not supported on this device");
            }
            var view = new MTKView(new CGRect(), device);
            var renderer = new Renderer(view);
            view.Delegate = renderer;

            _view = view;
            _renderer = renderer;
            base.View = _view;
        }
    }
}
