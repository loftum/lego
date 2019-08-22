using System;
using Foundation;
using AppKit;
using CoreGraphics;
using OpenTK.Platform.MacOS;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace Visualizer
{
    [Register("MainWindow")]
    public class MainWindow : NSWindow
    {
        public MonoMacGameView Game { get; set; }

        public MainWindow(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        [Export("initWithCoder:")]
        public MainWindow(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        public MainWindow(CGRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation)
            : base(contentRect, aStyle, bufferingType, deferCreation)
        {
            Initialize();
        }

        // Shared initialization code
        void Initialize()
        {
            Title = "Hest";
            Game = new MonoMacGameView(ContentView.Frame);
            //ContentView = Game;
            ContentView.AddSubview(Game);
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            // Wire-up any required Game events
            Game.Load += (sender, e) =>
            {
                // TODO: Initialize settings, load textures and sounds here
            };

            Game.Resize += (sender, e) =>
            {
                Console.WriteLine("resize");
                // Adjust the GL view to be the same size as the window
                GL.Viewport(0, 0, Game.Size.Width, Game.Size.Height);
            };

            Game.UpdateFrame += (sender, e) =>
            {
                // TODO: Add any game logic or physics
            };

            Game.RenderFrame += (sender, e) =>
            {
                // Setup buffer
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                GL.MatrixMode(MatrixMode.Projection);

                // Draw a simple triangle
                GL.LoadIdentity();
                GL.Ortho(-1.0, 1.0, -1.0, 1.0, 0.0, 4.0);
                GL.Begin(BeginMode.Triangles);
                GL.Color3(Color.MidnightBlue);
                GL.Vertex2(-1.0f, 1.0f);
                GL.Color3(Color.SpringGreen);
                GL.Vertex2(0.0f, -1.0f);
                GL.Color3(Color.Ivory);
                GL.Vertex2(1.0f, 1.0f);
                GL.End();

            };
            Console.WriteLine("agurk");
            // Run the game at 60 updates per second
            Game.Run(60.0);
            
        }
    }
}
