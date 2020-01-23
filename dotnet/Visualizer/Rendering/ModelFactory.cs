using System.Linq;
using Foundation;
using Metal;
using MetalKit;
using ModelIO;
using OpenTK;

namespace Visualizer.Rendering
{
    public static class ModelFactory
    {
        public static MTKMesh CreateRaceCar(IMTLLibrary library, MDLVertexDescriptor vertexDescriptor, MTKMeshBufferAllocator bufferAllocator)
        {
            return CreateFromModelFile(library, "3DModels/Aventador/Avent.obj", vertexDescriptor, bufferAllocator);
        }

        public static MTKMesh CreateTeapot(IMTLLibrary library, MDLVertexDescriptor vertexDescriptor, MTKMeshBufferAllocator bufferAllocator)
        {
            return CreateFromModelFile(library, "teapot.obj", vertexDescriptor, bufferAllocator);
        }

        public static MTKMesh CreateFromModelFile(IMTLLibrary library, string filename, MDLVertexDescriptor vertexDescriptor, MTKMeshBufferAllocator bufferAllocator)
        {
            var asset = new MDLAsset(NSUrl.FromFilename(filename), vertexDescriptor, bufferAllocator);
            var mesh = MTKMesh.FromAsset(asset, library.Device, out _, out var error);
            if (error != null)
            {
                throw new NSErrorException(error);
            }
            return mesh.First();
        }

        public static MTKMesh CreatePlane(IMTLLibrary library, MDLVertexDescriptor vertexDescriptor, MTKMeshBufferAllocator bufferAllocator)
        {
            var mdl = MDLMesh.CreatePlane(new Vector2(1f, 1f), new Vector2i(1, 1), MDLGeometryType.Triangles, bufferAllocator);
            mdl.VertexDescriptor = vertexDescriptor;
            var mesh = new MTKMesh(mdl, library.Device, out var error);
            if (error != null)
            {
                throw new NSErrorException(error);
            }
            return mesh;
        }

        public static MTKMesh CreateBox(IMTLLibrary library, MDLVertexDescriptor vertexDescriptor, MTKMeshBufferAllocator bufferAllocator)
        {
            var mdl = MDLMesh.CreateBox(new Vector3(1f, 1f, 1f), new Vector3i(1, 1, 1), MDLGeometryType.Triangles, false, bufferAllocator);
            mdl.VertexDescriptor = vertexDescriptor;
            var mesh = new MTKMesh(mdl, library.Device, out var error);
            
            if (error != null)
            {
                throw new NSErrorException(error);
            }
            return mesh;
        }
    }
}