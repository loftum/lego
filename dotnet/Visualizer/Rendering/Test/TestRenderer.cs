using System;
using System.Runtime.InteropServices;
using System.Threading;
using Metal;
using MetalKit;
using OpenTK;
using Visualizer.Rendering.Car.SceneGraph;

namespace Visualizer.Rendering.Test
{
    public class TestRenderer : MTKViewDelegate
    {
        private static readonly int VertexUniformsSize = Marshal.SizeOf<TestVertexUniforms>();
        private static readonly int FragmentUniformsSize = Marshal.SizeOf<TestFragmentUniforms>();

        private readonly MTKView _view;
        private readonly Semaphore _inflightSemaphore;
        private int _uniformsIndex;

        private readonly IMTLCommandQueue _commandQueue;

        private Matrix4 _projectionMatrix = Matrix4.Identity;
        private Matrix4 _viewMatrix = Matrix4.Identity;

        private readonly IMTLRenderPipelineState _renderPipeline;
        private readonly IMTLSamplerState _samplerState;
        private readonly IMTLDepthStencilState _depthStencilState;
        private readonly Scene _scene;
        private readonly Vector3 _cameraWorldPosition = new Vector3(0, 0, 5f);


        public TestRenderer(MTKView view)
        {
            // Set the view to use the default device
            var device = MTLDevice.SystemDefault ?? throw new Exception("Metal is not supported on this device");
            _uniformsIndex = 0;
            _inflightSemaphore = new Semaphore(TestRendererFactory.MaxInflightBuffers, TestRendererFactory.MaxInflightBuffers);

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
            var vertexDescriptor = TestRendererFactory.CreateVertexDescriptor();
            _renderPipeline = TestRendererFactory.CreateRenderPipeline(device, library, view, vertexDescriptor);
            _samplerState = TestRendererFactory.CreateSamplerState(device);
            _depthStencilState = TestRendererFactory.CreateDepthStencilState(device);

            _scene = TestRendererFactory.BuildScene(library, vertexDescriptor);
            Reshape();
        }

        public override void DrawableSizeWillChange(MTKView view, CoreGraphics.CGSize size)
        {
            Reshape();
        }

        private float _angle;
        private Vector3 GetRotation()
        {
            _angle += (float) (2 * Math.PI / 1000);
            if (_angle >= 2 * Math.PI)
            {
                _angle = 0;
            }
            // Camera on top of car
            return new Vector3(0, _angle, 0);
        }

        private void UpdateUniforms(MTKView view)
        {
            _projectionMatrix = CreateProjectionMatrix(view);

            var rotation = GetRotation();
            
            _scene.NodeNamed("car").ModelMatrix = Matrix4.Identity.Rotate(rotation * -2) * Matrix4.CreateTranslation(1f, 0, 0);
            _scene.NodeNamed("box").ModelMatrix = Matrix4.Identity.Rotate(rotation * .5f) * Matrix4.CreateTranslation(-1f, 0, 0); 
        }

        public override void Draw(MTKView view)
        {
            _inflightSemaphore.WaitOne();
            UpdateUniforms(view);

            // Create a new command buffer for each renderpass to the current drawable
            var commandBuffer = _commandQueue.CommandBuffer();
            commandBuffer.Label = "Hest";

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

            encoder.Label = "HestEncoder";
            encoder.SetFrontFacingWinding(MTLWinding.CounterClockwise);
            //encoder.SetCullMode(MTLCullMode.Back);
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

            _uniformsIndex = (_uniformsIndex + 1) % TestRendererFactory.MaxInflightBuffers;

            // Finalize rendering here & push the command buffer to the GPU
            commandBuffer.Commit();
        }

        private void DrawNodeRecursive(Node node, Matrix4 parentTransform, IMTLRenderCommandEncoder encoder)
        {
            var modelMatrix = parentTransform * node.ModelMatrix;
            var mesh = node.Mesh;
            if (mesh != null)
            {
                var vertexUniforms = new TestVertexUniforms
                {
                    ViewProjectionMatrix = _viewMatrix * _projectionMatrix,
                    ModelMatrix = modelMatrix,
                    NormalMatrix = node.ModelMatrix.Normal()
                };

                node.VertexUniformsBuffer.Copy(vertexUniforms, _uniformsIndex);

                encoder.SetVertexBuffer(node.VertexUniformsBuffer, (nuint)(VertexUniformsSize * _uniformsIndex), 1);

                var fragmentUniforms = new TestFragmentUniforms
                {
                    CameraWorldPosition = _cameraWorldPosition,
                    AmbientLightColor = _scene.AmbientLightColor * (float)Math.Sin(_angle),
                    SpecularColor = node.Material.SpecularColor,
                    SpecularPower = node.Material.SpecularPower,
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
            _viewMatrix = Matrix4.CreateTranslation(-_cameraWorldPosition);
            _projectionMatrix = CreateProjectionMatrix(_view);
        }

        private static Matrix4 CreateProjectionMatrix(MTKView view)
        {
            var aspectRatio = (float)(view.DrawableSize.Width / view.DrawableSize.Height);
            return Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 3, aspectRatio, .1f, 100f);
        }
    }
}
