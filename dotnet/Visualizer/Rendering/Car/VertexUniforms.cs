using System;
using System.Runtime.InteropServices;
using Maths;

namespace Visualizer.Rendering.Car
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexUniforms
    {
        public Float4x4 ViewProjectionMatrix;
        public Float4x4 ModelMatrix;
        public Float4x4 NormalMatrix;
    }
}