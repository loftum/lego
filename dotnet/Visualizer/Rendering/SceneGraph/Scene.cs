using System.Collections.Generic;
using Maths;

namespace Visualizer.Rendering.SceneGraph
{
    public class Scene
    {
        public Node RootNode { get; set; } = new Node("root");
        public Float3 AmbientLightColor { get; set; } = Float3.Zero;
        public List<Light> Lights { get; set; } = new List<Light>();

        public Node NodeNamed(string name)
        {
            return RootNode.Name == name
                ? RootNode
                : RootNode.NodeNamedRecursive(name);
        }
    }
}