using System;
using System.Collections.Generic;
using Lego.Core.Description;
using Maths;
using Metal;
using MetalKit;
using ModelIO;
using Visualizer.Rendering.SceneGraph;

namespace Visualizer.Rendering.Car
{
    public class SceneFactory
    {
        private readonly IMTLLibrary _library;
        private readonly MDLVertexDescriptor _vertexDescriptor;
        private readonly ModelFactory _modelFactory;
        private readonly int _maxInflightBuffers;

        public SceneFactory(IMTLLibrary library, MDLVertexDescriptor vertexDescriptor, int maxInflightBuffers)
        {
            _library = library;
            _vertexDescriptor = vertexDescriptor;
            _maxInflightBuffers = maxInflightBuffers;
            _modelFactory = new ModelFactory(library, _vertexDescriptor, new MTKMeshBufferAllocator(_library.Device));
        }

        public Scene BuildScene(LegoCarDescriptor descriptor)
        {
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

            // Default orientation: Car nose pointing -X
            //      ____
            //  ---/    \
            //  <--0---0--
            // Length: 5.5f, width: 2.5f
            const float raceCarLength = 5.5f;
            var scale = raceCarLength / descriptor.Length;
            const float raceCarWidth = 2.5f;
            
            var car = new Node("car")
            {
                Material = new Material { SpecularPower = 100f, SpecularColor = new Float3(.8f, .8f, .8f) },
                VertexUniformsBuffer = _library.Device.CreateBuffer((nuint)(VertexUniforms.SizeInBytes * _maxInflightBuffers), MTLResourceOptions.CpuCacheModeDefault),
                FragmentUniformsBuffer = _library.Device.CreateBuffer((nuint)(FragmentUniforms.SizeInBytes * _maxInflightBuffers), MTLResourceOptions.CpuCacheModeDefault),
                Mesh = _modelFactory.CreateRaceCar(),
                // Make car point upwards, looking down on the car roof
                InitialModelMatrix = Float4x4.Scale(.15f) * Float4x4.CreateRotation(-Float.PI / 2, 0, 0, 1) * Float4x4.CreateRotation(Float.PI / 2, 1, 0, 0)
            };
            car.VertexUniformsBuffer.Label = "Car VertexUniformsBuffer";
            car.FragmentUniformsBuffer.Label = "Car FragmentUniformsBuffer";
            Console.WriteLine($"VertexUniformsBuffer.length = {car.VertexUniformsBuffer.Length}");
            Console.WriteLine($"FragmentUniformsBuffer.length = {car.FragmentUniformsBuffer.Length}");
            scene.RootNode.Children.Add(car);

            var ii = 0;
            foreach (var distanceSensor in descriptor.DistanceSensors)
            {
                var distance = new Node($"distance {ii}")
                {
                    Material = new Material { SpecularPower = 100f, SpecularColor = new Float3(.8f, .8f, .8f) },
                    VertexUniformsBuffer = _library.Device.CreateBuffer((nuint)(VertexUniforms.SizeInBytes * _maxInflightBuffers), MTLResourceOptions.CpuCacheModeDefault),
                    FragmentUniformsBuffer = _library.Device.CreateBuffer((nuint)(FragmentUniforms.SizeInBytes * _maxInflightBuffers), MTLResourceOptions.CpuCacheModeDefault),
                    Mesh = _modelFactory.CreatePlane(1f / scale, 1f / scale),
                    // Flipping plane around Z-axis. Distances are translated along plane's Y axis
                    InitialModelMatrix = Float4x4.Scale(scale) * distanceSensor.ModelMatrix * Float4x4.CreateRotation(Float.PI / 2, 0, 0, 1f) 
                };
                car.Children.Add(distance);
                ii++;
            }
            
            return scene;
        }
    }
}