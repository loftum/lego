using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Foundation;
using Maths;
using Metal;
using MetalKit;
using ModelIO;
using Visualizer.Rendering.SceneGraph;

namespace Visualizer.Rendering.Test
{
    public class TestRendererFactory
    {
        public const int MaxInflightBuffers = 1;

        public static IMTLRenderPipelineState CreateRenderPipeline(IMTLDevice device, IMTLLibrary library, MTKView view, MDLVertexDescriptor vertexDescriptor)
        {
            var mtlVertexDescriptor = MTLVertexDescriptor.FromModelIO(vertexDescriptor);
            var vertexFunction = library.CreateFunction("vertex_test");
            var fragmentFunction = library.CreateFunction("fragment_test");
            var pipelineDescriptor = new MTLRenderPipelineDescriptor
            {
                Label = "RenderPipeline",
                SampleCount = view.SampleCount,
                VertexFunction = vertexFunction,
                FragmentFunction = fragmentFunction,
                DepthAttachmentPixelFormat = view.DepthStencilPixelFormat,
                //StencilAttachmentPixelFormat = view.DepthStencilPixelFormat,
                VertexDescriptor = mtlVertexDescriptor
            };
            pipelineDescriptor.ColorAttachments[0].PixelFormat = view.ColorPixelFormat;
            pipelineDescriptor.DepthAttachmentPixelFormat = view.DepthStencilPixelFormat;

            var state = device.CreateRenderPipelineState(pipelineDescriptor, out var error);
            if (error != null)
            {
                throw new NSErrorException(error);
            }

            return state;
        }

        public static IMTLSamplerState CreateSamplerState(IMTLDevice device)
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

        public static IMTLDepthStencilState CreateDepthStencilState(IMTLDevice device)
        {
            var depthStateDesc = new MTLDepthStencilDescriptor
            {
                DepthCompareFunction = MTLCompareFunction.Less,
                DepthWriteEnabled = true
            };

            return device.CreateDepthStencilState(depthStateDesc);
        }

        public static MDLVertexDescriptor CreateVertexDescriptor()
        {
            var vertextDescriptor = new MDLVertexDescriptor
            {
                Layouts = {[0] = new MDLVertexBufferLayout((nuint) (Marshal.SizeOf<float>() * 6))},
                Attributes =
                {
                    [0] = new MDLVertexAttribute(MDLVertexAttributes.Position.ToString(), MDLVertexFormat.Float3, 0, 0),
                    [1] = new MDLVertexAttribute(MDLVertexAttributes.Normal.ToString(), MDLVertexFormat.Float3, (nuint) (Marshal.SizeOf<float>() * 3), 0)
                }
            };
            //vertextDescriptor.Attributes[2] = new MDLVertexAttribute(MDLVertexAttributes.TextureCoordinate.ToString(), MDLVertexFormat.Float2, (nuint) (Marshal.SizeOf<float>() * 6), 0);
            return vertextDescriptor;
        }

        public static Scene BuildScene(IMTLLibrary library, MDLVertexDescriptor vertexDescriptor)
        {
            var bufferAllocator = new MTKMeshBufferAllocator(library.Device);
            var scene = new Scene
            {
                AmbientLightColor = new Float3(.5f, .5f, 0f),
                Lights = new List<Light>
                {
                    new Light {WorldPosition = new Float3(5, 5, 0), Color = new Float3(.3f, .3f, .3f)},
                    new Light {WorldPosition = new Float3(-5, 5, 0), Color = new Float3(.3f, .3f, .3f)},
                    new Light {WorldPosition = new Float3(0, -5, 0), Color = new Float3(.3f, .3f, .3f)}
                }
            };

            var car = new Node("car")
            {
                Material = new Material { SpecularPower = 100f, SpecularColor = new Float3(.8f, .8f, .8f) },
                VertexUniformsBuffer = library.Device.CreateBuffer((nuint)Marshal.SizeOf<TestVertexUniforms>() * MaxInflightBuffers, MTLResourceOptions.CpuCacheModeDefault),
                FragmentUniformsBuffer = library.Device.CreateBuffer((nuint)Marshal.SizeOf<TestFragmentUniforms>() * MaxInflightBuffers, MTLResourceOptions.CpuCacheModeDefault),
                Mesh = ModelFactory.CreateRaceCar(library, vertexDescriptor, bufferAllocator),
            };
            car.VertexUniformsBuffer.Label = "Car VertexUniformsBuffer";
            car.FragmentUniformsBuffer.Label = "Car FragmentUniformsBuffer";
            Console.WriteLine($"VertexUniformsBuffer.length = {car.VertexUniformsBuffer.Length}");
            Console.WriteLine($"FragmentUniformsBuffer.length = {car.FragmentUniformsBuffer.Length}");
            scene.RootNode.Children.Add(car);

            return scene;
        }
    }
}
