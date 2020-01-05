using Metal;
using OpenTK;

namespace Visualizer.Rendering.Car.SceneGraph
{
    public class Material
    {
        public float SpecularPower { get; set; } = 1f;
        public Vector3 SpecularColor { get; set; } = Vector3.One;
        public IMTLTexture BaseColorTexture { get; set; }
    }
}