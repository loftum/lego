using System;
using System.Collections.Generic;
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
        public Matrix4 normalMatrix { get; }
    }

    public class Renderer
    {
        private readonly MTKView mtkView;
        private readonly IMTLDevice device;
        private readonly IMTLCommandQueue commandQueue;

        private readonly MDLVertexDescriptor vertexDescriptor;
        private readonly IMTLRenderPipelineState renderPipeline;
        private readonly IMTLDepthStencilState depthStencilState;
        private List<MTKMesh> meshes = new List<MTKMesh>();
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
            //meshes = Renderer.loadResources(device: device, vertexDescriptor: vertexDescriptor)
            //baseColorTexture = Renderer.loadTexture(device: device)
            //depthStencilState = Renderer.createDepthStencilState(device: device)
            //samplerState = Renderer.createSamplerState(device: device)
        }

        private static MDLVertexDescriptor CreateVertexDescriptor()  {
            var vertexDescriptor = new MDLVertexDescriptor();
            vertexDescriptor.Attributes[0] = new MDLVertexAttribute(MDLVertexAttributes.Position, MDLVertexFormat.Float3, 0, 0);
            vertexDescriptor.Attributes[1] = new MDLVertexAttribute(MDLVertexAttributes.Normal, MDLVertexFormat.Float3, sizeof(float) * 3, 0);
            vertexDescriptor.Attributes[2] = new MDLVertexAttribute(MDLVertexAttributes.TextureCoordinate, MDLVertexFormat.Float2, sizeof(float) * 6, 0);
            vertexDescriptor.Layouts[0] = new MDLVertexBufferLayout(sizeof(float) * 8);
            return vertexDescriptor;
        }

        //private class func createSamplerState(device: MTLDevice) -> MTLSamplerState {
        //    let samplerDescriptor = MTLSamplerDescriptor()
        //    samplerDescriptor.normalizedCoordinates = true
        //    samplerDescriptor.minFilter = .linear
        //    samplerDescriptor.magFilter = .linear
        //    samplerDescriptor.mipFilter = .linear
        //    guard let samplerState = device.makeSamplerState(descriptor: samplerDescriptor) else {
        //        fatalError("Could not make sampler state")
        //    }
        //    return samplerState
        //}

        //private class func loadTexture(device: MTLDevice) -> MTLTexture {
        //    let textureLoader = MTKTextureLoader(device: device)
        //    let options: [MTKTextureLoader.Option: Any] = [.generateMipmaps: true, .SRGB: true]
        //    do {
        //        return try textureLoader.newTexture(name: "tiles_baseColor", scaleFactor: 1.0, bundle: nil, options: options)
        //    }
        //    catch {
        //        fatalError("Could not load texture: \(error)")
        //    }
        //}

        //private static func createDepthStencilState(device: MTLDevice) -> MTLDepthStencilState {
        //    let depthStencilDescriptor = MTLDepthStencilDescriptor()
        //    // Keep fragments closest to the camera
        //    depthStencilDescriptor.depthCompareFunction = .less
        //    depthStencilDescriptor.isDepthWriteEnabled = true // or else it doesn't work
        //    guard let state = device.makeDepthStencilState(descriptor: depthStencilDescriptor) else {
        //fatalError("Could not create depthStencilState")
        //    }
        //    return state
        //}

        private static IMTLRenderPipelineState BuildPipeline(IMTLDevice device, MTKView view, MDLVertexDescriptor vertexDescriptor) {
            // get library (collection of shader functions) from main bundle.
            // Shaders are defined in Shaders.metal
            var library = device.CreateDefaultLibrary();
            if (library == null) {
                throw new Exception("Could not load default library from main bundle");
            }

            var pipelineDescriptor = new MTLRenderPipelineDescriptor();
            pipelineDescriptor.VertexFunction = library.CreateFunction("vertex_main");
            pipelineDescriptor.FragmentFunction = library.CreateFunction("fragment_main");
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
    }
}
