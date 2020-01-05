using System.Collections.Generic;
using Metal;
using MetalKit;
using OpenTK;

namespace Visualizer.Rendering.Car.SceneGraph
{
    public class Node
    {
        public string Name { get; }
        public Node Parent { get; set; }
        public List<Node> Children { get; set; } = new List<Node>();
        public Matrix4 ModelMatrix { get; set; } = Matrix4.Identity;
        public MTKMesh Mesh { get; set; }
        public Material Material { get; set; }
        public IMTLBuffer VertexUniformsBuffer { get; set; }
        public IMTLBuffer FragmentUniformsBuffer { get; set; }

        public Node(string name)
        {
            Name = name;
        }

        public Node NodeNamedRecursive(string name)
        {
            foreach (var child in Children)
            {
                if (child.Name == name)
                {
                    return child;
                }

                var grandKid = child.NodeNamedRecursive(name);
                if (grandKid != null)
                {
                    return grandKid;
                }
            }
            return null;
        }
    }
}