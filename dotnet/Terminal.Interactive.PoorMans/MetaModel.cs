using System;
using System.Linq;
using System.Reflection;

namespace Terminal.Interactive.PoorMans
{
    public class MethodNode : IMetaNode
    {
        public string Name { get; }
        public object Owner { get; }
        public MethodInfo Method { get; }

        public MethodNode(object owner, MethodInfo method)
        {
            Name = method.Name;
            Owner = owner;
            Method = method;
        }

        public IMetaNode Traverse(string[] path)
        {
            if (!path.Any())
            {
                return this;
            }
            var value = GetValue();
            return new ObjectNode(Method.ReturnType, value);
        }

        public object GetValue()
        {
            throw new NotImplementedException();
        }
    }

    public class MetaModel
    {
        private readonly Type _type;

        public MetaModel(Type type)
        {
            _type = type;
        }

        public MemberInfo Traverse(string path, object owner)
        {
            
            var type = _type;
            MemberInfo member = null;
            var parts = path.Split('.');
            foreach (var part in parts)
            {
                if (type == null)
                {
                    throw new InvalidOperationException($"{member.Name} cannot be traversed further");
                }
                member = type.GetMembers().FirstOrDefault(m => string.Equals(m.Name, part, StringComparison.OrdinalIgnoreCase));
                if (member == null)
                {
                    return null;
                }
                type = member.GetTraverseType();
            }
            return member;
        }
    }
}