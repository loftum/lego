using System;
using System.Timers;
using AppKit;
using CoreFoundation;
using CoreGraphics;
using LCTP.Core.Client;
using Lego.Client;
using Lego.Core;
using Maths;
using Metal;
using MetalKit;
using Visualizer.Rendering.Car;

namespace Visualizer.ViewControllers
{
    public class VisualizerViewController : NSViewController, IRotationProvider, ILegoCarStateProvider
    {
        public event EventHandler OnDisconnect;
        private readonly NSSlider _throttleSlider;
        private readonly NSSlider _steerSlider;
        
        private LegoCarClient _client;
        private readonly IMTKViewDelegate _renderer;
        private readonly MTKView _mtkView;
        private readonly NSButton _disconnectButton;
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
            
            _disconnectButton = new NSButton
            {
                Title = "Disconnect",
                Cell =
                {
                    Bezeled = true
                }
            };
            _disconnectButton.Activated += Disconnect;
            
            View = new NSView()
                .WithSubview(_mtkView, (c, p) => new []
                {
                    c.CenterXAnchor.ConstraintEqualToAnchor(p.CenterXAnchor),
                    c.CenterYAnchor.ConstraintEqualToAnchor(p.CenterYAnchor),
                    c.WidthAnchor.ConstraintEqualToAnchor(p.WidthAnchor, .8f),
                    c.HeightAnchor.ConstraintEqualToAnchor(p.HeightAnchor, .8f)
                })
                .WithSubview(_throttleSlider, (c, p) => new[]
                {
                    c.TopAnchor.ConstraintEqualToAnchor(p.TopAnchor, 40),
                    c.TrailingAnchor.ConstraintEqualToAnchor(p.TrailingAnchor, -20),
                    c.BottomAnchor.ConstraintEqualToAnchor(p.BottomAnchor, -40)
                })
                .WithSubview(_steerSlider, (c, p) => new[]
                {
                    c.LeadingAnchor.ConstraintEqualToAnchor(p.LeadingAnchor, 40),
                    c.TrailingAnchor.ConstraintEqualToAnchor(p.TrailingAnchor, -40),
                    c.BottomAnchor.ConstraintEqualToAnchor(p.BottomAnchor, -20)
                })
                .WithSubview(_disconnectButton, (c, p) => new[]
                {
                    c.TopAnchor.ConstraintEqualToAnchor(p.TopAnchor, 20),
                    c.CenterXAnchor.ConstraintEqualToAnchor(p.CenterXAnchor)
                });
            _timer.Elapsed += Update;
            _timer.Start();
        }

        private async void Update(object sender, ElapsedEventArgs e)
        {
            if (_client == null)
            {
                return;
            }
            DispatchQueue.MainQueue.DispatchSync(() =>
            {
                _client.SetMotorSpeed(_throttleSlider.IntValue);
                _client.SetSteer(_steerSlider.IntValue);    
            });
            await _client.UpdateAsync();
        }

        public void Connect(string host, int port)
        {
            var client = new LctpUdpClient(host, port);
            _client = new LegoCarClient(client);
            _client.ConnectAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Stop();
                _timer.Elapsed -= Update;
                _disconnectButton.Activated -= Disconnect;
                _client.DisconnectAsync().Wait(1000);
                _client.Dispose();
                _renderer.Dispose();
                _mtkView.Dispose();
            }
            base.Dispose(disposing);
        }

        private void Disconnect(object sender, EventArgs e)
        {
            _client.DisconnectAsync().Wait(1000);
            OnDisconnect?.Invoke(this, EventArgs.Empty);
        }

        public LegoCarState GetState()
        {
            return _client?.GetState() ?? new LegoCarState();
        }

        public Double3 GetEulerAngles() => _client?.GetEulerAngles() ?? Double3.Zero;

        public Quatd GetQuaternion() => _client?.GetQuaternion() ?? new Quatd(0, 0, 0);

    }
}