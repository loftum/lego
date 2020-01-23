using System;
using System.Runtime.InteropServices;
using Maths;

namespace Visualizer.Rendering.Car
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct FragmentUniforms
    {
        public Float3 CameraWorldPosition;
        public Float3 AmbientLightColor;
        public Float3 SpecularColor;
        public float SpecularPower;
    }
}
