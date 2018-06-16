using System.Linq;
using System.Reflection;

namespace Terminal.Interactive.PoorMans
{
    public class PropertyNode : IMetaNode
    {
        public object Owner { get; }
        public PropertyInfo Property { get; }


        public PropertyNode(object owner, PropertyInfo property)
        {
            Owner = owner;
            Property = property;
        }

        public IMetaNode Traverse(string[] path)
        {
            if (!path.Any())
            {
                return this;
            }
            var value = GetValue();
            return new ObjectNode(Property.PropertyType, value).Traverse(path);
        }

        public object GetValue()
        {
            return Property.GetValue(Owner);
        }
    }
}