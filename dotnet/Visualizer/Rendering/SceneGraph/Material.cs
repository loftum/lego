using Maths;
using Metal;

namespace Visualizer.Rendering.SceneGraph
{
    public class Material
    {
        public float SpecularPower { get; set; } = 1f;
        public Float3 SpecularColor { get; set; } = Float3.One;
        public IMTLTexture BaseColorTexture { get; set; }
    }
}