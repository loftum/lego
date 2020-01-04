using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Foundation;
using Lego.Client;
using Metal;
using MetalKit;
using ModelIO;
using OpenTK;
using Visualizer.Rendering.SceneGraph;


namespace Visualizer.Rendering
{
    public struct VertexUniforms
    {
        public Matrix4 ViewProjecttionMatrix;
        public Matrix4 ModelMatrix;
        public Matrix3 NormalMatrix;
    }

    public struct FragmentUniforms
    {
        public Vector3 CameraWorldPosition;
        public Vector3 AmbientLightColor;
        public Vector3 SpecularColor;
        public float SpecularPower;
        public Light Light0;
        public Light Light1;
        public Light Light2;
    }
    
    public class CarRenderer : MTKViewDelegate
    {
        // The max number of command buffers in flight
        private const int MaxInflightBuffers = 3;

        private readonly MTKView _view;

        private readonly Semaphore _inflightSemaphore;
        private static readonly int UniformsSize = Marshal.SizeOf<Uniforms>();
        private readonly IMTLBuffer _uniformsBuffer;
        private int _uniformsIndex;
        private int _offset;

        private readonly IMTLCommandQueue _commandQueue;

        private Matrix4 _projectionMatrix;
        private Matrix4 _viewMatrix;
        private readonly IRotationProvider _positionProvider;

        private readonly IMTLRenderPipelineState _renderPipeline;
        private readonly IMTLSamplerState _samplerState;
        private readonly IMTLDepthStencilState _depthStaencilState;
        private readonly Scene _scene;
        private readonly MDLVertexDescriptor _vertexDescriptor;
        

        public CarRenderer(MTKView view, IRotationProvider positionProvider)
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
            
            view.Delegate = this;
            view.Device = device;

            
            // Setup the render target, choose values based on your app
            view.SampleCount = 4;
            view.DepthStencilPixelFormat = MTLPixelFormat.Depth32Float_Stencil8;
            
            _view = view;


            var library = device.CreateDefaultLibrary();
            //_meshes = LoadAssets(library);
            
            
            
            _commandQueue = device.CreateCommandQueue();
            _vertexDescriptor = CreateVertexDescriptor();
            _renderPipeline = CreateRenderPipeline(device, library, view, _vertexDescriptor);
            _samplerState = CreateSamplerState(device);
            _depthStaencilState = CreateDepthStencilState(device);
            
            _scene = BuildScene(library, _vertexDescriptor);
            
            Reshape();
        }

        private static IMTLRenderPipelineState CreateRenderPipeline(IMTLDevice device, IMTLLibrary library, MTKView view, MDLVertexDescriptor vertexDescriptor)
        {
            var mtlVertexDescriptor = MTLVertexDescriptor.FromModelIO(vertexDescriptor);
            var vertexFunction = library.CreateFunction("vertex_main");
            var fragmentFunction = library.CreateFunction("fragment_main");
            var pipelineDescriptor = new MTLRenderPipelineDescriptor
            {
                VertexFunction = vertexFunction,
                FragmentFunction = fragmentFunction,
                DepthAttachmentPixelFormat = view.DepthStencilPixelFormat,
                VertexDescriptor = mtlVertexDescriptor
            };
            pipelineDescriptor.ColorAttachments[0].PixelFormat = view.ColorPixelFormat;

            var state = device.CreateRenderPipelineState(pipelineDescriptor, out var error);
            if (error != null)
            {
                throw new NSErrorException(error);
            }

            return state;
        }

        private static IMTLSamplerState CreateSamplerState(IMTLDevice device)
        {
            var samplerDescriptor = new MTLSamplerDescriptor
            {
                NormalizedCoordinates = true,
                MinFilter = MTLSamplerMinMagFilter.Linear,
                MagFilter = MTLSamplerMinMagFilter.Linear,
                MipFilter = MTLSamplerMipFilter.Linear
            };
            return device.CreateSamplerState(samplerDescriptor);
        }

        private static IMTLDepthStencilState CreateDepthStencilState(IMTLDevice device)
        {
            var depthStateDesc = new MTLDepthStencilDescriptor
            {
                DepthCompareFunction = MTLCompareFunction.Less,
                DepthWriteEnabled = true
            };

            return device.CreateDepthStencilState(depthStateDesc);
        }

        private static MDLVertexDescriptor CreateVertexDescriptor()
        {
            var vertextDescriptor = new MDLVertexDescriptor
            {
                Attributes =
                {
                    [0] = new MDLVertexAttribute(MDLVertexAttributes.Position.ToString(), MDLVertexFormat.Float3, 0, 0),
                    [1] = new MDLVertexAttribute(MDLVertexAttributes.Normal.ToString(), MDLVertexFormat.Float3, (nuint) (Marshal.SizeOf<float>() * 3), 0),
                    [2] = new MDLVertexAttribute(MDLVertexAttributes.TextureCoordinate.ToString(), MDLVertexFormat.Float2, (nuint) (Marshal.SizeOf<float>() * 6), 0),
                },
                Layouts =
                {
                    [0] = new MDLVertexBufferLayout((nuint) (Marshal.SizeOf<float>() * 8))
                }
            };
            return vertextDescriptor;
        }

        private Scene BuildScene(IMTLLibrary library, MDLVertexDescriptor vertexDescriptor)
        {
            
            var bufferAllocator = new MTKMeshBufferAllocator(library.Device);


            var scene = new Scene
            {
                AmbientLightColor = new Vector3(.1f, .1f, .1f),
                Lights = new List<Light>
                {
                    new Light {WorldPosition = new Vector3(5, 5, 0), Color = new Vector3(.3f, .3f, .3f)},
                    new Light {WorldPosition = new Vector3(-5, 5, 0), Color = new Vector3(.3f, .3f, .3f)},
                    new Light {WorldPosition = new Vector3(0, -5, 0), Color = new Vector3(.3f, .3f, .3f)}
                }
            };

            var car = new Node("car")
            {
                Material = new Material
                {
                    SpecularPower = 100,
                    SpecularColor = new Vector3(.8f, .8f, .8f)
                }
            };
            
            var carAsset = new MDLAsset(NSUrl.FromFilename("teapot.obj"), vertexDescriptor, bufferAllocator);
            car.Mesh = MTKMesh.FromAsset(carAsset, library.Device, out _, out var error).First();
            if (error != null)
            {
                throw new NSErrorException(error);
            }
            scene.RootNode.Children.Add(car);

            return scene;
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        

        // private MTKMesh[] LoadAssets(IMTLLibrary library)
        // {
        //     // Generate meshes
        //     
        //     MDLMesh mdl = MDLMesh.CreateBox(new Vector3(2f, 2f, 2f), new Vector3i(1, 1, 1), MDLGeometryType.Triangles, false, new MTKMeshBufferAllocator(_device));
        //     //MDLMesh mdl = MDLMesh.CreateBox(new Vector3(1f, 2f, 2f), new Vector3i(1, 1, 1), MDLGeometryType.Triangles, false, new MTKMeshBufferAllocator(device));
        //     //var mdl = MDLMesh.CreateCylinder(new Vector3(2f, 2f, 2f), new Vector2i(1, 1), true, true, true, MDLGeometryType.Triangles, new MTKMeshBufferAllocator(device));
        //     //var mdl = MDLMesh.CreateEllipsoid(new Vector3(2f, 2f, 2f), 1, 1, MDLGeometryType.Triangles, true, true, new MTKMeshBufferAllocator(_device));
        //     var boxMesh = new MTKMesh(mdl, _device, out var error);
        //     if (error != null)
        //     {
        //         throw new NSErrorException(error);
        //     }
        //     
        //     
        //     // Create a vertex descriptor from the MTKMesh
        //     var vertexDescriptor = MTLVertexDescriptor.FromModelIO(boxMesh.VertexDescriptor);
        //     vertexDescriptor.Layouts[0].StepRate = 1;
        //     vertexDescriptor.Layouts[0].StepFunction = MTLVertexStepFunction.PerVertex;
        //
        //     var pipelineStateDescriptor = new MTLRenderPipelineDescriptor
        //     {
        //         Label = "MyPipeline",
        //         SampleCount = _view.SampleCount,
        //         VertexFunction = library.CreateFunction("lighting_vertex"),
        //         FragmentFunction = library.CreateFunction("lighting_fragment"),
        //         VertexDescriptor = vertexDescriptor,
        //         DepthAttachmentPixelFormat = _view.DepthStencilPixelFormat,
        //         StencilAttachmentPixelFormat = _view.DepthStencilPixelFormat
        //     };
        //
        //     pipelineStateDescriptor.ColorAttachments[0].PixelFormat = _view.ColorPixelFormat;
        //     _pipelineState = _device.CreateRenderPipelineState(pipelineStateDescriptor, out error);
        //
        //     if (error != null)
        //     {
        //         throw new NSErrorException(error);
        //     }
        //     return new[] { boxMesh };
        // }

        public override void DrawableSizeWillChange(MTKView view, CoreGraphics.CGSize size)
        {
            Reshape();
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
                drawable.Dispose();
                var count = _inflightSemaphore.Release();
            });
            
            

            var renderPassDescriptor = view.CurrentRenderPassDescriptor;
            if (renderPassDescriptor == null)
            {
                return;
            }
            
            renderPassDescriptor.ColorAttachments[0].ClearColor =  new MTLClearColor(0.63, 0.81, 1.0, 1.0);
            var commandEncoder = commandBuffer.CreateRenderCommandEncoder(renderPassDescriptor);
            
            
            // Create a render command encoder so we can render into something
            IMTLRenderCommandEncoder renderEncoder = commandBuffer.CreateRenderCommandEncoder(renderPassDescriptor);
            renderEncoder.Label = "MyRenderEncoder";
            //commandEncoder.SetFrontFacingWinding(MTLWinding.CounterClockwise);
            //commandEncoder.SetCullMode(MTLCullMode.Back);
            renderEncoder.SetDepthStencilState(_depthStaencilState);
            renderEncoder.SetRenderPipelineState(_renderPipeline);
            renderEncoder.SetFragmentSamplerState(_samplerState, 0);

            // Set context state
            renderEncoder.PushDebugGroup("DrawScene");
            DrawNodeRecursive(_scene.RootNode, Matrix4.Identity, commandEncoder);

            renderEncoder.PopDebugGroup();

            // We're done encoding commands
            renderEncoder.EndEncoding();

            // Schedule a present once the framebuffer is complete using the current drawable
            commandBuffer.PresentDrawable(drawable);

            _uniformsIndex = (_uniformsIndex + 1) % MaxInflightBuffers;

            // Finalize rendering here & push the command buffer to the GPU
            commandBuffer.Commit();
        }

        private void DrawNodeRecursive(Node node, Matrix4 parentTransform, IMTLRenderCommandEncoder commandEncoder)
        {
            var modelMatrix = parentTransform * node.ModelMatrix;
            var mesh = node.Mesh;
            if (mesh != null)
            {
                var viewProjectionMatrix = _projectionMatrix * _viewMatrix;
                var vertexUniforms = new VertexUniforms
                {
                    ViewProjecttionMatrix = viewProjectionMatrix,
                    ModelMatrix = modelMatrix,
                    NormalMatrix = modelMatrix.GetNormalMatrix()
                };
                ///
                ///
                /// 
            }
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
}