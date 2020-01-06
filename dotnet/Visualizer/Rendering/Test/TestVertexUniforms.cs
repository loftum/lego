using System;
using System.Runtime.InteropServices;
using OpenTK;

namespace Visualizer.Rendering.Test
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct TestVertexUniforms
    {
        public Matrix4 ViewProjectionMatrix;
        public Matrix4 NormalMatrix;
    }
}