using System;
using System.Runtime.InteropServices;
using System.Threading;
using Maths;
using Metal;
using MetalKit;
using Visualizer.Rendering.SceneGraph;

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

        private Float4x4 _projectionMatrix = Float4x4.Identity;
        private Float4x4 _viewMatrix = Float4x4.Identity;

        private readonly IMTLRenderPipelineState _renderPipeline;
        private readonly IMTLSamplerState _samplerState;
        private readonly IMTLDepthStencilState _depthStencilState;
        private readonly Scene _scene;
        private readonly Float3 _cameraWorldPosition = new Float3(0, 0, 5f);


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
        private Float3 GetRotation()
        {
            _angle += (float) (2 * Math.PI / 1000);
            if (_angle >= 2 * Math.PI)
            {
                _angle = 0;
            }
            // Camera on top of car
            return new Float3(0, _angle, 0);
        }

        private Quatf GetQuaternion()
        {
            _angle += (float) (2 * Math.PI / 1000);
            if (_angle >= 2 * Math.PI)
            {
                _angle = 0;
            }
            // Camera on top of car
            var wiggle = Float.Pi / 12 * Float.Sin(_angle * 12);
            return new Quatf(-Float.Pi / 2 + wiggle, 0, Float.Pi / 2 + _angle);
        }

        private void UpdateUniforms(MTKView view)
        {
            _projectionMatrix = CreateProjectionMatrix(view);

            
            var rotation = GetRotation();
            var quaternion = GetQuaternion();

            _scene.NodeNamed("car").ModelMatrix = Float4x4.Scale(.5f) * Float4x4.CreateFromQuaternion(quaternion);
            // _scene.NodeNamed("box").ModelMatrix = Float4x4.Identity.Rotate(rotation * .5f) * Float4x4.CreateTranslation(-1f, 0, 0);
            //_scene.NodeNamed("car").ModelMatrix = Float4x4.CreateTranslation(2f, 0, 0) * Float4x4.Identity.Rotate(rotation);
            //_scene.NodeNamed("box").ModelMatrix = Float4x4.CreateTranslation(-2f, 0, 0) * Float4x4.Identity.Rotate(rotation);
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
            DrawNodeRecursive(_scene.RootNode, Float4x4.Identity, encoder);

            encoder.PopDebugGroup();

            // We're done encoding commands
            encoder.EndEncoding();

            // Schedule a present once the framebuffer is complete using the current drawable
            commandBuffer.PresentDrawable(drawable);

            _uniformsIndex = (_uniformsIndex + 1) % TestRendererFactory.MaxInflightBuffers;

            // Finalize rendering here & push the command buffer to the GPU
            commandBuffer.Commit();
        }

        private void DrawNodeRecursive(Node node, Float4x4 parentTransform, IMTLRenderCommandEncoder encoder)
        {
            var modelMatrix = parentTransform * node.ModelMatrix;
            var mesh = node.Mesh;
            if (mesh != null)
            {
                var vertexUniforms = new TestVertexUniforms
                {
                    ViewProjectionMatrix = _projectionMatrix * _viewMatrix,
                    ModelMatrix = modelMatrix,
                    NormalMatrix = node.ModelMatrix.Transposed().Inverted()
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
            _viewMatrix = Float4x4.CreateTranslation(-_cameraWorldPosition);
            _projectionMatrix = CreateProjectionMatrix(_view);
        }

        private static Float4x4 CreateProjectionMatrix(MTKView view)
        {
            var aspectRatio = (float)(view.DrawableSize.Width / view.DrawableSize.Height);
            return Float4x4.CreatePerspectiveFieldOfView((float)Math.PI / 3, aspectRatio, .1f, 100f);
        }
    }
}
