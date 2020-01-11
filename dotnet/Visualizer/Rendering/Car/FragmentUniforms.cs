using System;
using System.Runtime.InteropServices;
using Maths;
using Visualizer.Rendering.Car.SceneGraph;

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
        public Light Light0;
        public Light Light1;
        public Light Light2;
    }
}