using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Terminal.Interactive.PoorMans
{
    public class ObjectNode : IMetaNode
    {
        public Type Type { get; }
        public object Value { get; }
        public IDictionary<string, IMetaNode> Members = new Dictionary<string, IMetaNode>(StringComparer.OrdinalIgnoreCase);

        public ObjectNode(Type type, object value)
        {
            Type = type;
            Value = value;
            foreach (var member in type.GetMembers())
            {
                Members[member.Name] = Parse(value, member);
            }
        }

        public IMetaNode Traverse(string[] path)
        {
            if (!path.Any())
            {
                return this;
            }
            var first = new Statement(path.First());
            var member = Members.TryGetValue(first.Name, out var node) ? node : null;
            return member?.Traverse(path.Skip(1).ToArray());
        }

        public object GetValue()
        {
            return Value;
        }

        public static IMetaNode Parse(object owner, MemberInfo member)
        {
            switch (member)
            {
                case PropertyInfo p: return new PropertyNode(owner, p);
                case FieldInfo f: return new FieldNode(owner, f);
                case MethodInfo m: return new MethodNode(owner, m);
            }
            return null;
        }
    }
}