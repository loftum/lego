using System;
using System.Collections.Generic;

namespace Maths.Logging
{
    public class ConsoleLogger : ILogger
    {
        private static readonly object Lock = new object();
        private static readonly Dictionary<MessageType, ConsoleColor> Colors = new Dictionary<MessageType, ConsoleColor>
        {
            [MessageType.Debug] = ConsoleColor.DarkGray,
            [MessageType.Info] = ConsoleColor.White,
            [MessageType.Warning] = ConsoleColor.Yellow,
            [MessageType.Error] = ConsoleColor.Red
        };

        private readonly string _name;

        public ConsoleLogger(string name)
        {
            _name = name;
        }

        public void Write(Importance importance, MessageType type, object message)
        {
            if (importance < Log.Level)
            {
                return;
            }
            lock (Lock)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = Colors[type];
                Console.WriteLine($"{_name}: {message}");
                Console.ForegroundColor = color;
            }
        }
    }
}