using System;
using System.Runtime.InteropServices;
using OpenTK;

namespace Visualizer.Rendering.Car
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexUniforms
    {
        public Matrix4 ViewProjectionMatrix;
        public Matrix4 ModelMatrix;
        public Matrix3 NormalMatrix;
    }
}