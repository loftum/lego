using System;
using System.Threading.Tasks;
using System.Timers;
using AppKit;
using CoreFoundation;
using CoreGraphics;
using Foundation;
using LCTP.Core.Client;
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
        
        private LegoCarClient _client;
        private readonly IMTKViewDelegate _renderer;
        private readonly MTKView _mtkView;
        private readonly NSButton _disconnectButton;
        private readonly NSTextField _xMotionField;
        private readonly NSTextField _yMotionField;
        private readonly InterlockedTimer _timer = new InterlockedTimer(25);

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
            _disconnectButton.Activated += DisconnectAsync;

            var xMotionLabel = new NSTextField {StringValue = "X-motion", Editable = false};
            _xMotionField = new NSTextField();
            var yMotionLabel = new NSTextField {StringValue = "Y-motion", Editable = false};
            _yMotionField = new NSTextField();
            View = new NSView()
                .WithSubview(_mtkView, (c, p) => new []
                {
                    c.CenterXAnchor.ConstraintEqualToAnchor(p.CenterXAnchor),
                    c.CenterYAnchor.ConstraintEqualToAnchor(p.CenterYAnchor),
                    c.WidthAnchor.ConstraintEqualToAnchor(p.WidthAnchor, .8f),
                    c.HeightAnchor.ConstraintEqualToAnchor(p.HeightAnchor, .8f)
                })
                .WithSubview(_disconnectButton, (c, p) => new[]
                {
                    c.TopAnchor.ConstraintEqualToAnchor(p.TopAnchor, 20),
                    c.CenterXAnchor.ConstraintEqualToAnchor(p.CenterXAnchor)
                })
                .WithSubview(new NSView()
                    .WithSubview(xMotionLabel, (c, p) => new []
                    {
                        c.LeadingAnchor.ConstraintEqualToAnchor(p.LeadingAnchor)
                    })
                    .WithSubview(_xMotionField, (c, p) => new[]
                    {
                        c.LeadingAnchor.ConstraintEqualToAnchor(xMotionLabel.TrailingAnchor),
                    })
                    .WithSubview(yMotionLabel, (c, p) => new []
                    {
                        c.LeadingAnchor.ConstraintEqualToAnchor(_xMotionField.TrailingAnchor)
                    })
                    .WithSubview(_yMotionField, (c, p) => new[]
                    {
                        c.LeadingAnchor.ConstraintEqualToAnchor(yMotionLabel.TrailingAnchor),
                        c.TrailingAnchor.ConstraintEqualToAnchor(p.TrailingAnchor)
                    }),
                    (c, p) => new[]
                    {
                        c.TopAnchor.ConstraintEqualToAnchor(_mtkView.BottomAnchor),
                        c.CenterXAnchor.ConstraintEqualToAnchor(p.CenterXAnchor),
                        c.WidthAnchor.ConstraintEqualToAnchor(p.WidthAnchor),
                        c.HeightAnchor.ConstraintEqualToAnchor(p.HeightAnchor),
                        c.BottomAnchor.ConstraintEqualToAnchor(p.BottomAnchor)
                    }
                )
                
                ;
            _timer.Elapsed += Update;
        }

        private async void Update(object sender, ElapsedEventArgs e)
        {
            if (_client == null || !_client.Connected)
            {
                return;
            }
            
            await _client.UpdateAsync();
            
        }

        public async Task ConnectAsync(string host, int port)
        {
            var client = new LctpClient(NSHost.Current.Name, host, port);
            _client = new LegoCarClient(client);
            
            await _client.ConnectAsync();
            _timer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Stop();
                _timer.Elapsed -= Update;
                _disconnectButton.Activated -= DisconnectAsync;
                _client.DisconnectAsync().Wait(1000);
                _client.Dispose();
                _renderer.Dispose();
                _mtkView.Dispose();
            }
            base.Dispose(disposing);
        }

        private async void DisconnectAsync(object sender, EventArgs e)
        {
            _timer.Stop();
            await _client.DisconnectAsync();
            OnDisconnect?.Invoke(this, EventArgs.Empty);
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