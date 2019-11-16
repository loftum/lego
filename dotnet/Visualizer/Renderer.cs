using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
        public Matrix4 modelMatrix { get; }
        public Matrix4 viewProjectionMatrix { get; }
        public Matrix3 normalMatrix { get; }

        public Uniforms(Matrix4 modelMatrix, Matrix4 viewProjectionMatrix, Matrix3 normalMatrix)
        {
            this.modelMatrix = modelMatrix;
            this.viewProjectionMatrix = viewProjectionMatrix;
            this.normalMatrix = normalMatrix;
        }
    }

    public class Renderer: MTKViewDelegate
    {
        private readonly MTKView mtkView;
        private readonly IMTLDevice device;
        private readonly IMTLCommandQueue commandQueue;

        private readonly MDLVertexDescriptor vertexDescriptor;
        private readonly IMTLRenderPipelineState renderPipeline;
        private readonly IMTLDepthStencilState depthStencilState;
        private MTKMesh[] meshes = new MTKMesh[0];
        private float time;

        private readonly IMTLTexture baseColorTexture;
        private readonly IMTLSamplerState samplerState;


        public Renderer(MTKView view)
        {
            mtkView = view;
            device = view.Device;
            commandQueue = device.CreateCommandQueue();
            vertexDescriptor = CreateVertexDescriptor();
            renderPipeline = BuildPipeline(device, view, vertexDescriptor);
            meshes = LoadResources(device, vertexDescriptor);
            baseColorTexture = LoadTexture(device);
            depthStencilState = CreateDepthStencilState(device);
            samplerState = CreateSamplerState(device);
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

        private static IMTLTexture LoadTexture(IMTLDevice device)
        {
            var textureLoader = new MTKTextureLoader(device);
            var options = new MTKTextureLoaderOptions(new NSDictionary
            {
                ["MTKTextureLoaderOptionGenerateMipmaps"] = NSObject.FromObject(true),
                ["MTKTextureLoaderOptionSRGB"] = NSObject.FromObject(true)
            });
            
            var texture = textureLoader.FromName("tiles_baseColor", (nfloat) 1.0, null, options, out var error);
            if (error != null)
            {
                throw new NSErrorException(error);
            }

            return texture;
        }

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
            //var url = NSBundle.GetUrlForResource("teapot", "obj", AppDomain.CurrentDomain.BaseDirectory, null);
            var file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "teapot.obj");
            var modelURL = new NSUrl($"file://{file}");

            var bufferAllocator = new MTKMeshBufferAllocator(device);
            var asset = new MDLAsset(modelURL, vertexDescriptor, bufferAllocator);
            var meshes = MTKMesh.FromAsset(asset, device, out _, out var error);
            if (error != null)
            {
                throw new NSErrorException(error);
            }

            return meshes;
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
            time += 1 / mtkView.PreferredFramesPerSecond;

            var commandBuffer = commandQueue.CommandBuffer();
            if (commandBuffer == null)
            {
                return;
            }

            // tells Metal which textures we will actually be drawing to.
            var renderPassDescriptor = view.CurrentRenderPassDescriptor;
            if (renderPassDescriptor == null)
            {
                return;
            }
            var drawable = view.CurrentDrawable;
            if (drawable == null)
            {
                return;
            }
            var commandEncoder = commandBuffer.CreateRenderCommandEncoder(renderPassDescriptor);
            if (commandEncoder == null)
            {
                return;
            }

            var uniforms = CreateUniforms(view, time);
            IntPtr intptr;
            Marshal.StructureToPtr(uniforms, intptr, false);

            commandEncoder.SetRenderPipelineState(renderPipeline);
            commandEncoder.SetDepthStencilState(depthStencilState);
            commandEncoder.SetVertexBytes(Marshal.StructureToPtr(uniforms, ), (nuint)Marshal.SizeOf<Uniforms>(), 1);
            commandEncoder.SetFragmentTexture(baseColorTexture, 0);
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
                    commandEncoder.draw(submesh);
                }
            }
            // tell encoder we will not be drawing any more things
            commandEncoder.EndEncoding();
            // callback to present the result
            commandBuffer.PresentDrawable(drawable);
            // ship commands to GPU
            commandBuffer.Commit();
        }
    }
    
    public static class MTLRenderCommandEncoderExtensions {
        // tells Metal to render a sequence of primitivies (shapes)
        public static void draw(this IMTLRenderCommandEncoder self, MTKSubmesh submesh)
        {
            self.DrawIndexedPrimitives(submesh.PrimitiveType, // triangle, line, point etc
                indexCount: submesh.IndexCount,
                indexType: submesh.IndexType,
                indexBuffer: submesh.IndexBuffer.Buffer,
                indexBufferOffset: submesh.IndexBuffer.Offset);
        }
    }
}
