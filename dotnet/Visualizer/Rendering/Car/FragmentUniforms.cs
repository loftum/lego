using OpenTK;
using Visualizer.Rendering.Car.SceneGraph;

namespace Visualizer.Rendering.Car
{
    public struct FragmentUniforms
    {
        public Vector3 CameraWorldPosition;
        public Vector3 AmbientLightColor;
        public Vector3 SpecularColor;
        public float SpecularPower;
        public Light Light0;
        public Light Light1;
        public Light Light2;
    }
}