using System.Linq;
using Foundation;
using Metal;
using MetalKit;
using ModelIO;
using OpenTK;

namespace Visualizer.Rendering
{
    public class ModelFactory
    {
        private readonly IMTLLibrary _library;
        private readonly MDLVertexDescriptor _vertexDescriptor;
        private readonly MTKMeshBufferAllocator _bufferAllocator;

        public ModelFactory(IMTLLibrary library, MDLVertexDescriptor vertexDescriptor, MTKMeshBufferAllocator bufferAllocator)
        {
            _library = library;
            _vertexDescriptor = vertexDescriptor;
            _bufferAllocator = bufferAllocator;
        }

        public MTKMesh CreateRaceCar()
        {
            return CreateFromModelFile("3DModels/Aventador/Avent.obj");
        }

        public MTKMesh CreateTeapot()
        {
            return CreateFromModelFile("teapot.obj");
        }

        public MTKMesh CreateFromModelFile(string filename)
        {
            var asset = new MDLAsset(NSUrl.FromFilename(filename), _vertexDescriptor, _bufferAllocator);
            var mesh = MTKMesh.FromAsset(asset, _library.Device, out _, out var error);
            if (error != null)
            {
                throw new NSErrorException(error);
            }
            return mesh.First();
        }

        public MTKMesh CreatePlane()
        {
            var mdl = MDLMesh.CreatePlane(new Vector2(1f, 1f), new Vector2i(1, 1), MDLGeometryType.Triangles, _bufferAllocator);
            mdl.VertexDescriptor = _vertexDescriptor;
            var mesh = new MTKMesh(mdl, _library.Device, out var error);
            if (error != null)
            {
                throw new NSErrorException(error);
            }
            return mesh;
        }

        public MTKMesh CreateBox()
        {
            var mdl = MDLMesh.CreateBox(new Vector3(1f, 1f, 1f), new Vector3i(1, 1, 1), MDLGeometryType.Triangles, false, _bufferAllocator);
            mdl.VertexDescriptor = _vertexDescriptor;
            var mesh = new MTKMesh(mdl, _library.Device, out var error);
            
            if (error != null)
            {
                throw new NSErrorException(error);
            }
            return mesh;
        }
    }
}