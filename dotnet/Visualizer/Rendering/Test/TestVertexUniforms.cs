using System;
using System.Runtime.InteropServices;
using Maths;

namespace Visualizer.Rendering.Test
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct TestVertexUniforms
    {
        public Float4x4 ViewProjectionMatrix;
        public Float4x4 ModelMatrix;
        public Float4x4 NormalMatrix;
    }
}