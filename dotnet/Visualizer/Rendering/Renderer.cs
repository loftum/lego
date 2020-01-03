using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Foundation;
using Lego.Client;
using Metal;
using MetalKit;
using ModelIO;
using OpenTK;

namespace Visualizer.Rendering
{
    public class Renderer : MTKViewDelegate
    {
        // The max number of command buffers in flight
        private const int MaxInflightBuffers = 1;

        // view
        private readonly MTKView _view;

        // controller
        private readonly Semaphore _inflightSemaphore;
        private static readonly int UniformsSize = Marshal.SizeOf<Uniforms>();
        private readonly IMTLBuffer _uniformsBuffer;
        private int _uniformsIndex;
        private int _offset;

        // renderer
        private readonly IMTLDevice _device;
        private readonly IMTLCommandQueue _commandQueue;
        private IMTLRenderPipelineState _pipelineState;
        private readonly IMTLDepthStencilState _depthStaencilState;

        // uniforms
        private Matrix4 _projectionMatrix;
        private Matrix4 _viewMatrix;
        private readonly MTKMesh[] _meshes;
        private readonly IRotationProvider _positionProvider;

        public Renderer(MTKView view, IRotationProvider positionProvider)
        {
            _positionProvider = positionProvider;
            // Set the view to use the default device
            var device = MTLDevice.SystemDefault;
            if (device == null)
            {
                throw new Exception("Metal is not supported on this device");
            }

            _uniformsIndex = 0;
            _inflightSemaphore = new Semaphore(MaxInflightBuffers, MaxInflightBuffers);

            _uniformsBuffer = device.CreateBuffer((nuint)UniformsSize * MaxInflightBuffers, MTLResourceOptions.CpuCacheModeDefault);
            _uniformsBuffer.Label = "UniformBuffer";

            // Create a new command queue
            _commandQueue = device.CreateCommandQueue();
            view.Delegate = this;
            view.Device = device;

            
            // Setup the render target, choose values based on your app
            view.SampleCount = 4;
            view.DepthStencilPixelFormat = MTLPixelFormat.Depth32Float_Stencil8;
            
            _view = view;
            _device = device;
            var depthStateDesc = new MTLDepthStencilDescriptor
            {
                DepthCompareFunction = MTLCompareFunction.Less,
                DepthWriteEnabled = true
            };

            _depthStaencilState = _device.CreateDepthStencilState(depthStateDesc);
            var library = device.CreateDefaultLibrary();
            _meshes = LoadAssets(library);
            Reshape();
        }

        private MTKMesh[] LoadAssets(IMTLLibrary library)
        {
            // Generate meshes
            MDLMesh mdl = MDLMesh.CreateBox(new Vector3(2f, 2f, 2f), new Vector3i(1, 1, 1), MDLGeometryType.Triangles, false, new MTKMeshBufferAllocator(_device));
            //MDLMesh mdl = MDLMesh.CreateBox(new Vector3(1f, 2f, 2f), new Vector3i(1, 1, 1), MDLGeometryType.Triangles, false, new MTKMeshBufferAllocator(device));
            //var mdl = MDLMesh.CreateCylinder(new Vector3(2f, 2f, 2f), new Vector2i(1, 1), true, true, true, MDLGeometryType.Triangles, new MTKMeshBufferAllocator(device));
            //var mdl = MDLMesh.CreateEllipsoid(new Vector3(2f, 2f, 2f), 1, 1, MDLGeometryType.Triangles, true, true, new MTKMeshBufferAllocator(_device));
            var boxMesh = new MTKMesh(mdl, _device, out var error);
            if (error != null)
            {
                throw new NSErrorException(error);
            }
            
            
            // Create a vertex descriptor from the MTKMesh
            var vertexDescriptor = MTLVertexDescriptor.FromModelIO(boxMesh.VertexDescriptor);
            vertexDescriptor.Layouts[0].StepRate = 1;
            vertexDescriptor.Layouts[0].StepFunction = MTLVertexStepFunction.PerVertex;

            var pipelineStateDescriptor = new MTLRenderPipelineDescriptor
            {
                Label = "MyPipeline",
                SampleCount = _view.SampleCount,
                VertexFunction = library.CreateFunction("lighting_vertex"),
                FragmentFunction = library.CreateFunction("lighting_fragment"),
                VertexDescriptor = vertexDescriptor,
                DepthAttachmentPixelFormat = _view.DepthStencilPixelFormat,
                StencilAttachmentPixelFormat = _view.DepthStencilPixelFormat
            };

            pipelineStateDescriptor.ColorAttachments[0].PixelFormat = _view.ColorPixelFormat;
            _pipelineState = _device.CreateRenderPipelineState(pipelineStateDescriptor, out error);

            if (error != null)
            {
                throw new NSErrorException(error);
            }
            return new[] { boxMesh };
        }

        public override void DrawableSizeWillChange(MTKView view, CoreGraphics.CGSize size)
        {
            Reshape();
        }

        public override void Draw(MTKView view)
        {
            _inflightSemaphore.WaitOne();
            UpdateUniforms();

            // Create a new command buffer for each renderpass to the current drawable
            IMTLCommandBuffer commandBuffer = _commandQueue.CommandBuffer();
            commandBuffer.Label = "MyCommand";

            // Call the view's completion handler which is required by the view since it will signal its semaphore and set up the next buffer
            var drawable = view.CurrentDrawable;
            commandBuffer.AddCompletedHandler(buffer =>
            {
                drawable.Dispose();
                var count = _inflightSemaphore.Release();
            });

            var renderPassDescriptor = view.CurrentRenderPassDescriptor;
            if (renderPassDescriptor != null)
            {
                // Create a render command encoder so we can render into something
                IMTLRenderCommandEncoder renderEncoder = commandBuffer.CreateRenderCommandEncoder(renderPassDescriptor);
                renderEncoder.Label = "MyRenderEncoder";
                renderEncoder.SetDepthStencilState(_depthStaencilState);

                // Set context state
                renderEncoder.PushDebugGroup("DrawCube");
                renderEncoder.SetRenderPipelineState(_pipelineState);
                foreach(var mesh in _meshes)
                {
                    renderEncoder.SetVertexBuffer(mesh.VertexBuffers[0].Buffer, mesh.VertexBuffers[0].Offset, 0);

                    renderEncoder.SetVertexBuffer(_uniformsBuffer, (nuint)_offset, 1);

                    var submesh = mesh.Submeshes[0];
                    // Tell the render context we want to draw our primitives
                    renderEncoder.DrawIndexedPrimitives(submesh.PrimitiveType, submesh.IndexCount, submesh.IndexType, submesh.IndexBuffer.Buffer, submesh.IndexBuffer.Offset);
                }

                renderEncoder.PopDebugGroup();

                // We're done encoding commands
                renderEncoder.EndEncoding();

                // Schedule a present once the framebuffer is complete using the current drawable
                commandBuffer.PresentDrawable(drawable);
            }

            _uniformsIndex = (_uniformsIndex + 1) % MaxInflightBuffers;

            // Finalize rendering here & push the command buffer to the GPU
            commandBuffer.Commit();
        }

        private Vector3 GetRotation()
        {
            var rotation = _positionProvider.GetRotation();
            // Camera on top of carq
            return new Vector3(-(float)rotation.Y, (float)rotation.Z, -(float)rotation.X);
        }

        private void UpdateUniforms()
        {
            var rotation = GetRotation();

            //var baseModel = Matrix4.Mult(M.CreateMatrixFromTranslation(0f, 0f, 5f), M.CreateMatrixFromRotation(rotation, 1f, 0f, 0f));
            var baseModel = M.CreateMatrixFromTranslation(0f, 0f, 5f).Rotate(rotation);
            var baseMv = Matrix4.Mult(_viewMatrix, baseModel);
            var modelViewMatrix = Matrix4.Mult(baseMv, M.CreateMatrixFromRotation(0, 1f, 1f, 1f));

            var uniforms = new Uniforms
            {
                NormalMatrix = Matrix4.Invert(Matrix4.Transpose(modelViewMatrix)),
                ModelviewProjectionMatrix = Matrix4.Transpose(Matrix4.Mult(_projectionMatrix, modelViewMatrix))
            };

            var rawdata = new byte[UniformsSize];

            GCHandle pinnedUniforms = GCHandle.Alloc(uniforms, GCHandleType.Pinned);
            IntPtr ptr = pinnedUniforms.AddrOfPinnedObject();
            Marshal.Copy(ptr, rawdata, 0, UniformsSize);
            pinnedUniforms.Free();
            _offset = UniformsSize * _uniformsIndex;

            Marshal.Copy(rawdata, 0, _uniformsBuffer.Contents + _offset, UniformsSize);
        }
        
        

        private void Reshape()
        {
            // When reshape is called, update the view and projection matricies since this means the view orientation or size changed
            var aspect = (float)(_view.Bounds.Size.Width / _view.Bounds.Size.Height);
            _projectionMatrix = M.CreateMatrixFromPerspective(65f * ((float)Math.PI / 180f), aspect, .1f, 100f);

            _viewMatrix = Matrix4.Identity;
        }

        
    }

    public static class M
    {
        public static Matrix4 Rotate(this Matrix4 matrix, Vector3 angles)
        {
            var rotations = new[]
            {
                CreateMatrixFromRotation(angles.X, 1, 0, 0),
                CreateMatrixFromRotation(angles.Y, 0, 1, 0),
                CreateMatrixFromRotation(angles.Z, 0, 0, 1)
            };
            return rotations.Aggregate(matrix, Matrix4.Mult);
        }
        
        public static Matrix4 CreateMatrixFromRotation(float radians, float x, float y, float z)
        {
            Vector3 v = Vector3.Normalize(new Vector3(x, y, z));
            var cos = (float)Math.Cos(radians);
            var sin = (float)Math.Sin(radians);
            float cosp = 1f - cos;

            var m = new Matrix4
            {
                Row0 = new Vector4(cos + cosp * v.X * v.X, cosp * v.X * v.Y - v.Z * sin, cosp * v.X * v.Z + v.Y * sin, 0f),
                Row1 = new Vector4(cosp * v.X * v.Y + v.Z * sin, cos + cosp * v.Y * v.Y, cosp * v.Y * v.Z - v.X * sin, 0f),
                Row2 = new Vector4(cosp * v.X * v.Z - v.Y * sin, cosp * v.Y * v.Z + v.X * sin, cos + cosp * v.Z * v.Z, 0f),
                Row3 = new Vector4(0f, 0f, 0f, 1f)
            };

            return m;
        }

        public static Matrix4 CreateMatrixFromTranslation(float x, float y, float z)
        {
            var m = Matrix4.Identity;
            m.Row0.W = x;
            m.Row1.W = y;
            m.Row2.W = z;
            m.Row3.W = 1f;
            return m;
        }

        public static Matrix4 CreateMatrixFromPerspective(float fovY, float aspect, float nearZ, float farZ)
        {
            float yscale = 1f / (float)Math.Tan(fovY * .5f);
            float xscale = yscale / aspect;
            float q = farZ / (farZ - nearZ);

            var m = new Matrix4
            {
                Row0 = new Vector4(xscale, 0f, 0f, 0f),
                Row1 = new Vector4(0f, yscale, 0f, 0f),
                Row2 = new Vector4(0f, 0f, q, q * -nearZ),
                Row3 = new Vector4(0f, 0f, 1f, 0f)
            };

            return m;
        }
    }
}
