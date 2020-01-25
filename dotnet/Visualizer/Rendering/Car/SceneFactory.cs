using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Maths;
using Metal;
using MetalKit;
using ModelIO;
using Visualizer.Rendering.SceneGraph;

namespace Visualizer.Rendering.Car
{
    public static class SceneFactory
    {
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
                VertexUniformsBuffer = library.Device.CreateBuffer((nuint)Marshal.SizeOf<VertexUniforms>() * CarRendererFactory.MaxInflightBuffers, MTLResourceOptions.CpuCacheModeDefault),
                FragmentUniformsBuffer = library.Device.CreateBuffer((nuint)Marshal.SizeOf<FragmentUniforms>() * CarRendererFactory.MaxInflightBuffers, MTLResourceOptions.CpuCacheModeDefault),
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