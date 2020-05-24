using System.Linq;

namespace LCTP.Core
{
    public class RequestMessage : IMessage
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string Content { get; set; }

        public static RequestMessage Parse(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return null;
            }
            var parts = raw.Split(' ');
            return new RequestMessage
            {
                Method = parts[0],
                Path = parts[1],
                Content = string.Join(" ", parts.Skip(2))
            };
        }

        public string Format()
        {
            return string.IsNullOrEmpty(Content)
                ? $"{Method} {Path}"
                : $"{Method} {Path} {Content}";
        }

        public override string ToString()
        {
            return Format();
        }

        public static RequestMessage Ping => new RequestMessage
        {
            Method = "PING",
            Path = "/"
        };

        public static RequestMessage Connect => new RequestMessage
        {
            Method = "CONNECT",
            Path = "/"
        };
        
        public static RequestMessage Disconnect => new RequestMessage
        {
            Method = "DISCONNECT",
            Path = "/"
        };
    }
}