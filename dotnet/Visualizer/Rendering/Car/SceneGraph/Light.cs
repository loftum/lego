using System;
using System.Runtime.InteropServices;
using Maths;

namespace Visualizer.Rendering.Car.SceneGraph
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Light
    {
        public Float3 WorldPosition;
        public Float3 Color;
    }
}