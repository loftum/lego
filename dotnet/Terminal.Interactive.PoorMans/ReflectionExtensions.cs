using System;
using System.Reflection;

namespace Terminal.Interactive.PoorMans
{
    internal static class ReflectionExtensions
    {
        public static Type GetTraverseType(this MemberInfo member)
        {
            switch (member)
            {
                case PropertyInfo p: return p.PropertyType;
                case FieldInfo f: return f.FieldType;
            }
            return null;
        }

        public static object GetValue(this MemberInfo member, object parameter)
        {
            switch (member)
            {
                case PropertyInfo p: return p.GetValue(parameter);
                case FieldInfo f: return f.IsStatic ? f.GetValue(null) : f.GetValue(parameter);
            }
            throw new InvalidOperationException($"Cannot get value from {member.MemberType}");
        }
    }
}