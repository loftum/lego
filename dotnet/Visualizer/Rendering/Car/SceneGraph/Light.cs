using System;
using System.Runtime.InteropServices;
using OpenTK;

namespace Visualizer.Rendering.Car.SceneGraph
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct Light
    {
        public Vector3 WorldPosition;
        public Vector3 Color;
    }
}