using System.Collections.Generic;
using OpenTK;

namespace Visualizer.Rendering.Car.SceneGraph
{
    public class Scene
    {
        public Node RootNode { get; set; } = new Node("root");
        public Vector3 AmbientLightColor { get; set; } = Vector3.Zero;
        public List<Light> Lights { get; set; } = new List<Light>();

        public Node NodeNamed(string name)
        {
            return RootNode.Name == name
                ? RootNode
                : RootNode.NodeNamedRecursive(name);
        }
    }
}