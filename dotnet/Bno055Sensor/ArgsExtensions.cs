using System;
using System.Collections.Generic;
using System.Linq;

namespace Bno055Sensor
{
    internal static class ArgsExtensions
    {
        public static bool Has(this IEnumerable<string> args, string arg)
        {
            return args.Any(a => arg.StartsWith(a, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}