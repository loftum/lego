using System;
using System.Threading.Tasks;
using AppKit;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using Lego.Client;
using Lego.Core;
using Lego.Core.Description;
using Maths;
using Metal;
using MetalKit;
using Visualizer.Rendering.Car;

namespace Visualizer.ViewControllers
{
    public class VisualizerViewController : NSViewController, IRotationProvider, ILegoCarStateProvider
    {
        public event EventHandler OnDisconnect;
        
        private readonly LegoCarClient _client = new LegoCarClient(NSHost.Current.Name);
        private readonly IMTKViewDelegate _renderer;
        private readonly MTKView _mtkView;
        private readonly NSButton _disconnectButton;
        private readonly NSTextField _xMotionField;
        private readonly NSTextField _yMotionField;

        public VisualizerViewController()
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
            _renderer = new CarRenderer(_mtkView, this);
            _mtkView.Delegate = _renderer;
            
            _disconnectButton = new NSButton
            {
                Title = "Disconnect",
                Cell =
                {
                    Bezeled = true
                }
            };
            _disconnectButton.Activated += DisconnectButtonClicked;

            var xMotionLabel = new NSTextField {StringValue = "X-motion", Editable = false};
            _xMotionField = new NSTextField { Editable = false, StringValue = "     " };
            var yMotionLabel = new NSTextField {StringValue = "Y-motion", Editable = false};
            _yMotionField = new NSTextField { Editable = false, StringValue = "     " };
            

            var numberView = new NSStackView {Orientation = NSUserInterfaceLayoutOrientation.Horizontal}
                .WithArrangedSubviews(xMotionLabel, _xMotionField, yMotionLabel, _yMotionField);
            
            View = new NSView()
                .WithSubview(_disconnectButton, (c, p) => new[]
                {
                    c.TopAnchor.ConstraintEqualToAnchor(p.TopAnchor, 20),
                    c.CenterXAnchor.ConstraintEqualToAnchor(p.CenterXAnchor)
                })
                .WithSubview(_mtkView, (c, p) => new []
                {
                    c.TopAnchor.ConstraintEqualToAnchor(_disconnectButton.BottomAnchor),
                    c.LeadingAnchor.ConstraintEqualToAnchor(p.LeadingAnchor),
                    c.TrailingAnchor.ConstraintEqualToAnchor(p.TrailingAnchor)
                })
                .WithSubview(numberView, (c, p) => new[]
                {
                    c.TopAnchor.ConstraintEqualToAnchor(_mtkView.BottomAnchor),
                    c.LeadingAnchor.ConstraintEqualToAnchor(p.LeadingAnchor),
                    c.TrailingAnchor.ConstraintEqualToAnchor(p.TrailingAnchor),
                    c.BottomAnchor.ConstraintEqualToAnchor(p.BottomAnchor)
                })
                ;
            _client.DidUpdate = DidUpdate;
            _client.Disconnected = _ => DisconnectAsync();
        }

        private Task DidUpdate(LegoCarClient client, LegoCarState state)
        {
            DispatchQueue.MainQueue.DispatchAsync(() =>
            {
                _xMotionField.IntValue = state.Motion.X;
                _yMotionField.IntValue = state.Motion.Y;
            });
            return Task.CompletedTask;
        }

        public async Task ConnectAsync(string host, int port)
        {
            await _client.ConnectAsync(host, port);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disconnectButton.Activated -= DisconnectButtonClicked;
                _client.DisconnectAsync().Wait(1000);
                _client.Dispose();
                _renderer.Dispose();
                _mtkView.Dispose();
            }
            base.Dispose(disposing);
        }

        private async Task DisconnectAsync()
        {
            await _client.DisconnectAsync();
            OnDisconnect?.Invoke(this, EventArgs.Empty);
        }

        private async void DisconnectButtonClicked(object sender, EventArgs e)
        {
            await DisconnectAsync();
        }

        public LegoCarState GetState()
        {
            return _client?.GetState() ?? new LegoCarState();
        }

        public LegoCarDescriptor GetCarDescriptor() => LegoCarDescriptor.RaceCar();

        public Double3 GetEulerAngles() => _client?.GetEulerAngles() ?? Double3.Zero;

        public Quatd GetQuaternion() => _client?.GetQuaternion() ?? new Quatd(0, 0, 0);

    }
}