using System;
using System.Runtime.InteropServices;
using OpenTK;

namespace Visualizer.Rendering.Test
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct TestFragmentUniforms
    {
        public Vector3 CameraWorldPosition;
        public Vector3 AmbientLightColor;
        public Vector3 SpecularColor;
        public float SpecularPower;
    }
}
