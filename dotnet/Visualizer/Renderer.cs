using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using CoreGraphics;
using CoreText;
using Foundation;
using Metal;
using MetalKit;
using ModelIO;
using OpenTK;

namespace Visualizer
{
    struct Uniforms
    {
        public Matrix4 ModelMatrix { get; }
        public Matrix4 ViewProjectionMatrix { get; }
        public Matrix3 NormalMatrix { get; }

        public Uniforms(Matrix4 modelMatrix, Matrix4 viewProjectionMatrix, Matrix3 normalMatrix)
        {
            this.ModelMatrix = modelMatrix;
            this.ViewProjectionMatrix = viewProjectionMatrix;
            this.NormalMatrix = normalMatrix;
        }
    }

    public class Renderer: MTKViewDelegate
    {
        private readonly Semaphore _inflightSemaphore = new Semaphore(MaxInflightBuffers, MaxInflightBuffers);
        private const int MaxInflightBuffers = 3;
        private readonly MTKView mtkView;
        private readonly IMTLDevice device;
        private readonly IMTLCommandQueue commandQueue;

        private readonly MDLVertexDescriptor vertexDescriptor;
        private readonly IMTLRenderPipelineState renderPipeline;
        private readonly IMTLDepthStencilState depthStencilState;
        private MTKMesh[] meshes = new MTKMesh[0];
        private float time;

        //private readonly IMTLTexture baseColorTexture;
        private readonly IMTLSamplerState samplerState;
        private readonly IMTLBuffer _uniformsBuffer;
        private static readonly int UniformsSize = Marshal.SizeOf<Uniforms>();
        private int _uniformsBufferIndex;
        private int _uniformsOffset;


        public Renderer(MTKView view)
        {
            Console.WriteLine("Renderer ctor");
            mtkView = view;
            device = view.Device;
            commandQueue = device.CreateCommandQueue();
            vertexDescriptor = CreateVertexDescriptor();
            renderPipeline = BuildPipeline(device, view, vertexDescriptor);
            meshes = LoadResources(device, vertexDescriptor);
            //baseColorTexture = LoadTexture(device);
            depthStencilState = CreateDepthStencilState(device);
            samplerState = CreateSamplerState(device);
            _uniformsBuffer = device.CreateBuffer((nuint)UniformsSize * MaxInflightBuffers, 0);
            _uniformsBuffer.Label = "UniformsBuffer";    
        }

        private static MDLVertexDescriptor CreateVertexDescriptor()  {
            var vertexDescriptor = new MDLVertexDescriptor
            {
                Attributes =
                {
                    [0] = new MDLVertexAttribute(MDLVertexAttributes.Position, MDLVertexFormat.Float3, 0, 0),
                    [1] = new MDLVertexAttribute(MDLVertexAttributes.Normal, MDLVertexFormat.Float3, sizeof(float) * 3, 0),
                    [2] = new MDLVertexAttribute(MDLVertexAttributes.TextureCoordinate, MDLVertexFormat.Float2, sizeof(float) * 6, 0)
                },
                Layouts = {[0] = new MDLVertexBufferLayout(sizeof(float) * 8)}
            };
            return vertexDescriptor;
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
            var samplerState = device.CreateSamplerState(samplerDescriptor);
            if (samplerState == null)
            {
                throw new Exception("Could not make sampler state");
            }

            return samplerState;
        }

        //private static IMTLTexture LoadTexture(IMTLDevice device)
        //{
        //    var textureLoader = new MTKTextureLoader(device);
        //    var options = new MTKTextureLoaderOptions();
        //    options.Srgb = true;
            
        //    var texture = textureLoader.FromName("tiles_baseColor", (nfloat) 1.0, null, options, out var error);
        //    if (error != null)
        //    {
        //        throw new NSErrorException(error);
        //    }

        //    return texture;
        //}

        private static IMTLDepthStencilState CreateDepthStencilState(IMTLDevice device)
        {
            var depthStencilDescriptor = new MTLDepthStencilDescriptor
            {
                DepthCompareFunction = MTLCompareFunction.Less, // Keep fragments closest to the camera
                DepthWriteEnabled = true // or else it doesn't work
            };
            
            var state = device.CreateDepthStencilState(depthStencilDescriptor);
            if (state == null)
            {
                throw new Exception("Could not create depthStencilState");
            }

            return state;
        }
        
        private static MTKMesh[] LoadResources(IMTLDevice device, MDLVertexDescriptor vertexDescriptor)
        {
            //var modelURL = NSBundle.MainBundle.GetUrlForResource("teapot.obj", string.Empty);
            //var modelURL = NSBundle.GetUrlForResource("teapot", "obj", AppDomain.CurrentDomain.BaseDirectory);
            //var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "teapot.obj");
            //var modelURL = new NSUrl($"file://{file}");

            //var bufferAllocator = new MTKMeshBufferAllocator(device);
            //Console.WriteLine("Newing up asset");
            //var asset = new MDLAsset(modelURL, vertexDescriptor, bufferAllocator);
            //Console.WriteLine("Creating meshes");
            //var meshes = MTKMesh.FromAsset(asset, device, out _, out var error);
            //if (error != null)
            //{
            //    throw new NSErrorException(error);
            //}
            //Console.WriteLine("Loaded resources");

            //return meshes;

            MDLMesh mdl = MDLMesh.CreateBox(new Vector3(2f, 2f, 2f), new Vector3i(1, 1, 1), MDLGeometryType.Triangles, false, new MTKMeshBufferAllocator(device));

            NSError error;
            var boxMesh = new MTKMesh(mdl, device, out error);
            if (error != null)
            {
                throw new NSErrorException(error);
            }

            return new[]
            {
                boxMesh
            };

        }

        private static IMTLRenderPipelineState BuildPipeline(IMTLDevice device, MTKView view, MDLVertexDescriptor vertexDescriptor)
        {
            // get library (collection of shader functions) from main bundle.
            // Shaders are defined in Shaders.metal
            var library = device.CreateDefaultLibrary();
            if (library == null) {
                throw new Exception("Could not load default library from main bundle");
            }

            var pipelineDescriptor = new MTLRenderPipelineDescriptor
            {
                VertexFunction = library.CreateFunction("vertex_main"),
                FragmentFunction = library.CreateFunction("fragment_main")
            };
            // tell Metal the format (pixel layout) of the textures we will be drawing to
            pipelineDescriptor.ColorAttachments[0].PixelFormat = view.ColorPixelFormat;
            pipelineDescriptor.DepthAttachmentPixelFormat = view.DepthStencilPixelFormat;
            pipelineDescriptor.VertexDescriptor = MTLVertexDescriptor.FromModelIO(vertexDescriptor);

            var pipeline = device.CreateRenderPipelineState(pipelineDescriptor, out var error);
            if (error != null) {
                throw new NSErrorException(error);
            }
            return pipeline;
        }

        public override void DrawableSizeWillChange(MTKView view, CGSize size)
        {
            
        }

        
        private static Uniforms CreateUniforms(MTKView view, float time)
        {
            var angle = -time;
            var modelMatrix = Matrix4.CreateFromAxisAngle(new Vector3(0, 1, 0), angle) * Matrix4.Scale(2);
            // describes camera position
            var viewMatrix = Matrix4.CreateTranslation(new Vector3(0, 0, -2));

            var aspectRatio = (float) (view.DrawableSize.Width / view.DrawableSize.Height);
            // Anything nearer than .1 units and further away than 100 units will bel clipped (not visible)
            var projectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI / 3), aspectRatio, 0.1f, 100f);
        
            var viewProjectionMatrix = projectionMatrix * viewMatrix;
            return new Uniforms(modelMatrix, viewProjectionMatrix, modelMatrix.GetNormalMatrix()); // create extension
        }
        
        
        
        public override void Draw(MTKView view)
        {
            _inflightSemaphore.WaitOne();
            time += 1 / mtkView.PreferredFramesPerSecond;

            var commandBuffer = commandQueue.CommandBuffer();
            if (commandBuffer == null)
            {
                Console.WriteLine("oh noes 1");
                return;
            }
            var drawable = view.CurrentDrawable;
            if (drawable == null)
            {
                Console.WriteLine("oh noes 3");
                return;
            }

            commandBuffer.AddCompletedHandler(buffer =>
            {
                drawable.Dispose();
                _inflightSemaphore.Release();
            });

            // tells Metal which textures we will actually be drawing to.
            var renderPassDescriptor = view.CurrentRenderPassDescriptor;
            if (renderPassDescriptor == null)
            {
                Console.WriteLine("oh noes 2");
                return;
            }
            
            
            var commandEncoder = commandBuffer.CreateRenderCommandEncoder(renderPassDescriptor);
            if (commandEncoder == null)
            {
                Console.WriteLine("oh noes 4");
                return;
            }
            UpdateUniforms(view);

            commandEncoder.SetRenderPipelineState(renderPipeline);
            commandEncoder.SetDepthStencilState(depthStencilState);
            commandEncoder.SetVertexBuffer(_uniformsBuffer, (nuint) _uniformsOffset, 1);
            //commandEncoder.SetVertexBytes(ptr, (nuint)Marshal.SizeOf<Uniforms>(), 1);
            //commandEncoder.SetVertexBuffer Bytes(&uniforms, Marshal.StructureToPtr(uniforms, ), (nuint)Marshal.SizeOf<Uniforms>(), 1);
            //commandEncoder.SetFragmentTexture(baseColorTexture, 0);
            commandEncoder.SetFragmentSamplerState(samplerState, 0);

            foreach (var mesh in meshes)
            {
                var vertexBuffer = mesh.VertexBuffers.FirstOrDefault();
                if (vertexBuffer == null)
                {
                    continue;
                }

                commandEncoder.SetVertexBuffer(vertexBuffer.Buffer, vertexBuffer.Offset, 0);

                foreach(var submesh in mesh.Submeshes)
                {
                    commandEncoder.Draw(submesh);
                }
            }
            // tell encoder we will not be drawing any more things
            commandEncoder.EndEncoding();
            // callback to present the result
            commandBuffer.PresentDrawable(drawable);

            _uniformsBufferIndex = (_uniformsBufferIndex + 1) % MaxInflightBuffers;
            _uniformsOffset = UniformsSize * _uniformsBufferIndex;
            // ship commands to GPU
            commandBuffer.Commit();
        }

        private void UpdateUniforms(MTKView view)
        {
            var uniforms = CreateUniforms(view, time);
            var rawData = new byte[UniformsSize];
            GCHandle pinnedUniforms = GCHandle.Alloc(uniforms, GCHandleType.Pinned);
            var ptr = pinnedUniforms.AddrOfPinnedObject();
            Marshal.Copy(ptr, rawData, 0, UniformsSize);
            pinnedUniforms.Free();


            Marshal.Copy(rawData, 0, _uniformsBuffer.Contents + _uniformsOffset, UniformsSize);
        }
    }
    
    public static class MTLRenderCommandEncoderExtensions {
        // tells Metal to render a sequence of primitivies (shapes)
        public static void Draw(this IMTLRenderCommandEncoder self, MTKSubmesh submesh)
        {
            self.DrawIndexedPrimitives(submesh.PrimitiveType, // triangle, line, point etc
                indexCount: submesh.IndexCount,
                indexType: submesh.IndexType,
                indexBuffer: submesh.IndexBuffer.Buffer,
                indexBufferOffset: submesh.IndexBuffer.Offset);
        }
    }
}
