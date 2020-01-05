using OpenTK;

namespace Visualizer.Rendering.Car
{
    public struct VertexUniforms
    {
        public Matrix4 ViewProjectionMatrix;
        public Matrix4 ModelMatrix;
        public Matrix3 NormalMatrix;
    }
}