using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LCTP.Core.Routing
{
    public class Route
    {
        public string Method { get; }
        public string Path { get; }
        public Func<RequestMessage, Match, Task<ResponseMessage>> Handler { get; }

        public Route(string method, string path, Func<RequestMessage, Match, Task<ResponseMessage>> handler)
        {
            Method = method;
            Path = path;
            Handler = handler;
        }

        public Func<RequestMessage, Task<ResponseMessage>> GetHandler(RequestMessage request)
        {
            if (!string.Equals(request.Method, Method, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            var match = Regex.Match(request.Path, Path);
            return !match.Success
                ? (Func<RequestMessage, Task<ResponseMessage>>) null
                : r => Handler(r, match);
        }

        public override bool Equals(object obj)
        {
            return obj is Route route &&
                   Method == route.Method &&
                   Path == route.Path;
        }

        public override int GetHashCode()
        {
            var hashCode = 657874982;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Method);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);
            return hashCode;
        }
    }
}