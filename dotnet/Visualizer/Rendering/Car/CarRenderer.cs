using System;
using System.Runtime.InteropServices;
using System.Threading;
using Lego.Client;
using Metal;
using MetalKit;
using OpenTK;
using Visualizer.Rendering.Car.SceneGraph;

namespace Visualizer.Rendering.Car
{
    public class CarRenderer : MTKViewDelegate
    {
        private static readonly int VertexUniformsSize = Marshal.SizeOf<VertexUniforms>();
        private static readonly int FragmentUniformsSize = Marshal.SizeOf<FragmentUniforms>();

        private readonly MTKView _view;
        private readonly Semaphore _inflightSemaphore;
        private int _uniformsIndex;

        private readonly IMTLCommandQueue _commandQueue;

        private Matrix4 _projectionMatrix = Matrix4.Identity;
        private Matrix4 _viewMatrix = Matrix4.Identity;
        private Matrix4 _viewProjectionMatrix = Matrix4.Identity;
        private readonly IRotationProvider _positionProvider;

        private readonly IMTLRenderPipelineState _renderPipeline;
        private readonly IMTLSamplerState _samplerState;
        private readonly IMTLDepthStencilState _depthStencilState;
        private readonly Scene _scene;
        private readonly Vector3 _cameraWorldPosition = new Vector3(0, 0, 20);
        

        public CarRenderer(MTKView view, IRotationProvider positionProvider)
        {
            _positionProvider = positionProvider;
            // Set the view to use the default device
            var device = MTLDevice.SystemDefault ?? throw new Exception("Metal is not supported on this device");
            _uniformsIndex = 0;
            _inflightSemaphore = new Semaphore(CarRendererFactory.MaxInflightBuffers, CarRendererFactory.MaxInflightBuffers);
            
            view.Delegate = this;
            view.Device = device;
            view.ColorPixelFormat = MTLPixelFormat.BGRA8Unorm_sRGB;

            // Setup the render target, choose values based on your app
            view.SampleCount = 4;
            //view.DepthStencilPixelFormat = MTLPixelFormat.Depth32Float;
            view.DepthStencilPixelFormat = MTLPixelFormat.Depth32Float_Stencil8;

            _view = view;

            var library = device.CreateDefaultLibrary();

            _commandQueue = device.CreateCommandQueue();
            var vertexDescriptor = CarRendererFactory.CreateVertexDescriptor();
            _renderPipeline = CarRendererFactory.CreateRenderPipeline(device, library, view, vertexDescriptor);
            _samplerState = CarRendererFactory.CreateSamplerState(device);
            _depthStencilState = CarRendererFactory.CreateDepthStencilState(device);

            _scene = CarRendererFactory.BuildScene(library, vertexDescriptor);
            Reshape();
        }

        public override void DrawableSizeWillChange(MTKView view, CoreGraphics.CGSize size)
        {
            Reshape();
        }

        private Vector3 GetRotation()
        {
            var rotation = _positionProvider.GetRotation();
            // Camera on top of car
            return new Vector3(-(float)rotation.Y, (float)rotation.Z, -(float)rotation.X);
        }

        private void UpdateUniforms()
        {
            var rotation = GetRotation();
            var node = _scene.NodeNamed("car");
            node.ModelMatrix = Matrix4.Identity; //.Rotate(rotation);
        }

        public override void Draw(MTKView view)
        {
            _inflightSemaphore.WaitOne();
            UpdateUniforms();

            // Create a new command buffer for each renderpass to the current drawable
            var commandBuffer = _commandQueue.CommandBuffer();
            commandBuffer.Label = "MyCommand";

            // Call the view's completion handler which is required by the view since it will signal its semaphore and set up the next buffer
            var drawable = view.CurrentDrawable;
            commandBuffer.AddCompletedHandler(buffer =>
            {
                var index = _uniformsIndex;
                drawable.Dispose();
                var count = _inflightSemaphore.Release();
            });


            var renderPassDescriptor = view.CurrentRenderPassDescriptor;
            if (renderPassDescriptor == null)
            {
                return;
            }
            //renderPassDescriptor.ColorAttachments[0].ClearColor = new MTLClearColor(0.63, 0.81, 1.0, 1.0);
            var encoder = commandBuffer.CreateRenderCommandEncoder(renderPassDescriptor);

            //// Create a render command encoder so we can render into something

            encoder.Label = "MyRenderEncoder";
            encoder.SetFrontFacingWinding(MTLWinding.CounterClockwise);
            encoder.SetCullMode(MTLCullMode.Back);
            encoder.SetDepthStencilState(_depthStencilState);
            encoder.SetRenderPipelineState(_renderPipeline);
            encoder.SetFragmentSamplerState(_samplerState, 0);

            //// Set context state
            encoder.PushDebugGroup("DrawScene");
            DrawNodeRecursive(_scene.RootNode, Matrix4.Identity, encoder);

            encoder.PopDebugGroup();

            // We're done encoding commands
            encoder.EndEncoding();

            // Schedule a present once the framebuffer is complete using the current drawable
            commandBuffer.PresentDrawable(drawable);

            _uniformsIndex = (_uniformsIndex + 1) % CarRendererFactory.MaxInflightBuffers;

            // Finalize rendering here & push the command buffer to the GPU
            commandBuffer.Commit();
        }

        private void DrawNodeRecursive(Node node, Matrix4 parentTransform, IMTLRenderCommandEncoder encoder)
        {
            var modelMatrix = parentTransform * node.ModelMatrix;
            var mesh = node.Mesh;
            if (mesh != null)
            {
                //var viewProjectionMatrix = _projectionMatrix * _viewMatrix;
                var vertexUniforms = new VertexUniforms
                {
                    ViewProjectionMatrix = _viewProjectionMatrix,
                    ModelMatrix = modelMatrix,
                    NormalMatrix = modelMatrix.GetNormalMatrix()
                };
                
                node.VertexUniformsBuffer.Copy(vertexUniforms, _uniformsIndex);
                
                encoder.SetVertexBuffer(node.VertexUniformsBuffer, (nuint) (VertexUniformsSize * _uniformsIndex), 1);

                var fragmentUniforms = new FragmentUniforms
                {
                    CameraWorldPosition = _cameraWorldPosition,
                    AmbientLightColor = _scene.AmbientLightColor,
                    SpecularColor = node.Material.SpecularColor,
                    SpecularPower = node.Material.SpecularPower,
                    Light0 = _scene.Lights[0],
                    Light1 = _scene.Lights[1],
                    Light2 = _scene.Lights[2]
                };

                node.FragmentUniformsBuffer.Copy(fragmentUniforms, _uniformsIndex);
                encoder.SetFragmentBuffer(node.FragmentUniformsBuffer, (nuint)(FragmentUniformsSize * _uniformsIndex), 0);

                encoder.SetVertexBuffer(mesh.VertexBuffers[0].Buffer, mesh.VertexBuffers[0].Offset, 0);

                foreach (var submesh in mesh.Submeshes)
                {
                    encoder.DrawIndexedPrimitives(submesh.PrimitiveType, submesh.IndexCount, submesh.IndexType, submesh.IndexBuffer.Buffer, submesh.IndexBuffer.Offset);
                }
            }

            foreach (var child in node.Children)
            {
                DrawNodeRecursive(child, modelMatrix, encoder);
            }
        }

        private void Reshape()
        {
            _viewMatrix = Matrix4.CreateTranslation(-_cameraWorldPosition); // * Matrix4.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)(Math.PI / 6));
            var aspectRatio = (float)(_view.DrawableSize.Width / _view.DrawableSize.Height);
            //_projectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI / 6), aspectRatio, .1f, 100f);
            _projectionMatrix = M.CreateMatrixFromPerspective(65f * ((float)Math.PI / 180f), aspectRatio, .1f, 100f);
            _viewProjectionMatrix = _projectionMatrix * _viewMatrix;
        }
    }

    public static class MtlBufferExtensions
    {
        public static void Copy<T>(this IMTLBuffer buffer, T uniforms, int bufferIndex) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var rawdata = new byte[size];

            var pinnedUniforms = GCHandle.Alloc(uniforms, GCHandleType.Pinned);
            var ptr = pinnedUniforms.AddrOfPinnedObject();
            Marshal.Copy(ptr, rawdata, 0, size);
            pinnedUniforms.Free();
            var offset = size * bufferIndex;

            Marshal.Copy(rawdata, 0, buffer.Contents + offset, size);
        }
    }
}