using System;
using System.Collections.Concurrent;

namespace Maths.Logging
{
    internal class IdGenerator
    {
        private readonly object _lock = new object();
        private int _id;
        public int Next()
        {
            lock (_lock)
            {
                return ++_id;    
            }
        }
    }
    
    public static class Log
    {
        private static readonly ConcurrentDictionary<string, IdGenerator> Ids = new ConcurrentDictionary<string, IdGenerator>();
        
        public static Importance Level { get; set; } = Importance.Normal;

        public static ILogger For(object owner) => For(owner.GetType());
        public static ILogger For<T>() => For(typeof(T));
        public static ILogger For(Type type) => For(type.Name);
        public static ILogger For(string name) => new ConsoleLogger(GetName(name));

        private static string GetName(string prefix)
        {
            var id = Ids.GetOrAdd(prefix, _ => new IdGenerator());
            return $"{prefix} {id.Next()}";
        }
    }
}