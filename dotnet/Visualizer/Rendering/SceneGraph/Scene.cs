using System.Collections.Generic;
using Metal;
using MetalKit;
using OpenTK;

namespace Visualizer.Rendering.SceneGraph
{
    public struct Light
    {
        public Vector3 WorldPosition { get; set; }
        public Vector3 Color { get; set; }
    }
    
    public class Material
    {
        public float SpecularPower { get; set; } = 1f;
        public Vector3 SpecularColor { get; set; } = Vector3.One;
        public IMTLTexture BaseColorTexture { get; set; }
    }
    
    public class Node
    {
        public string Name { get; }
        public Node Parent { get; set; }
        public List<Node> Children { get; set; } = new List<Node>();
        public Matrix4 ModelMatrix { get; set; } = Matrix4.Identity;
        public MTKMesh Mesh { get; set; }
        public Material Material { get; set; }

        public Node(string name)
        {
            Name = name;
        }

        public Node NodeNamedRecursive(string name)
        {
            foreach (var node in Children)
            {
                if (node.Name != name)
                {
                    return node;
                }

                var grandKid = node.NodeNamedRecursive(name);
                if (grandKid != null)
                {
                    return grandKid;
                }
            }
            return null;
        }
    }
    
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