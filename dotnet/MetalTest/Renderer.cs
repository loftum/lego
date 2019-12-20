using System;
using System.Linq;
using System.Runtime.InteropServices;
using CoreGraphics;
using Metal;
using MetalKit;
using OpenTK;

namespace MetalTest
{
    public static class FloatExtensions
    {
        public static byte[] ToBytes(this float[] floats)
        {
            return floats.SelectMany(BitConverter.GetBytes)
                .ToArray();
        }
    }

    public struct Vertex
    {
        public Vector4 Color { get; }
        public Vector2 Pos { get; }

        public Vertex(Vector4 color, Vector2 pos)
        {
            Color = color;
            Pos = pos;
        }
    }

    public class Renderer : MTKViewDelegate
    {
        private readonly IMTLDevice _device;
        private readonly IMTLCommandQueue _commandQueue;
        private readonly IMTLRenderPipelineState _pipelineState;
        private readonly IMTLBuffer _vertexBuffer;

        public Renderer(MTKView view)
        {
            _device = view.Device;
            _commandQueue = _device.CreateCommandQueue();
            _pipelineState = BuildRenderPipeline(view.Device, view);

            var vertices = new[]
            {
                new Vertex(new Vector4(1, 0, 0, 1), new Vector2(-1, -1)),
                new Vertex(new Vector4(0, 1, 0, 1), new Vector2(0, 1)),
                new Vertex(new Vector4(0, 0, 1, 1), new Vector2(1, -1)),
            };
            _vertexBuffer = _device.CreateBuffer(vertices, 0);
            
            var rawsize = Marshal.SizeOf<Vertex>();
            for (var ii = 0; ii < vertices.Length; ii++)
            {
                var vertex = vertices[ii];
                var rawdata = new byte[rawsize];
                var pinnedUniforms = GCHandle.Alloc(vertex, GCHandleType.Pinned);
                var ptr = pinnedUniforms.AddrOfPinnedObject();
                Marshal.Copy(ptr, rawdata, 0, rawsize);
                pinnedUniforms.Free();

                Marshal.Copy(rawdata, 0, _vertexBuffer.Contents + rawsize * ii, rawsize);
            }
        }

        private static IMTLRenderPipelineState BuildRenderPipeline(IMTLDevice device, MTKView view)
        {
            var library = device.CreateDefaultLibrary();
            var pipelineDescriptor = new MTLRenderPipelineDescriptor
            {
                VertexFunction = library.CreateFunction("vertexShader"),
                FragmentFunction = library.CreateFunction("fragmentShader")
            };
            pipelineDescriptor.ColorAttachments[0].PixelFormat = view.ColorPixelFormat;
            var result = device.CreateRenderPipelineState(pipelineDescriptor, out var error);
            if (error != null)
            {
                throw new Exception($"Building pipeline failed: {error}");
            }
            return result;
        }

        public override void Draw(MTKView view)
        {
            var renderPassDescriptor = view.CurrentRenderPassDescriptor;
            renderPassDescriptor.ColorAttachments[0].ClearColor = new MTLClearColor(1, 0, 0, 1);
            var commandBuffer = _commandQueue.CommandBuffer();
            var renderEncoder = commandBuffer.CreateRenderCommandEncoder(renderPassDescriptor);
            // more commands to come

            renderEncoder.SetRenderPipelineState(_pipelineState);
            renderEncoder.SetVertexBuffer(_vertexBuffer, 0, 0);
            renderEncoder.DrawPrimitives(MTLPrimitiveType.Triangle, 0, 3);

            // Finalizes encoding of drawing commands. Does _not_ send commands to GPU.
            renderEncoder.EndEncoding();
            // Tell Metal to send the rendering result to the MTKView when rendering completes
            commandBuffer.PresentDrawable(view.CurrentDrawable);

            // Queues qcommands to GPU.
            commandBuffer.Commit();
        }

        public override void DrawableSizeWillChange(MTKView view, CGSize size)
        {

        }
    }
}
