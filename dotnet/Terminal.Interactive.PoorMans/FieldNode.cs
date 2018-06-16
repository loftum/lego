using System.Linq;
using System.Reflection;

namespace Terminal.Interactive.PoorMans
{
    public class FieldNode : IMetaNode
    {
        public object Owner { get; }
        public FieldInfo Field { get; }

        public FieldNode(object owner, FieldInfo field)
        {
            Owner = owner;
            Field = field;
        }

        public IMetaNode Traverse(string[] path)
        {
            if (!path.Any())
            {
                return this;
            }
            var value = GetValue();
            return new ObjectNode(Field.FieldType, value).Traverse(path);
        }

        public object GetValue()
        {
            return Field.GetValue(Owner);
        }
    }
}