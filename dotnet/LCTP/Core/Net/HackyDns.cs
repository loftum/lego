using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LCTP.Core.Net
{
    public static class HackyDns
    {
        public static IEnumerable<IPAddress> GetLocalAddresses()
        {
            return GetHostAddresses(Dns.GetHostName())
                .Concat(Dns.GetHostAddresses("127.0.0.1"));
        }
        
        public static IEnumerable<IPAddress> GetHostAddresses(string host)
        {
            try
            {
                return Dns.GetHostAddresses(host)
                    .Concat(TryGetHostAddresses($"{host}.local"));
            }
            catch
            {
                return Enumerable.Empty<IPAddress>();
            }
        }

        private static IEnumerable<IPAddress> TryGetHostAddresses(string host)
        {
            try
            {
                return Dns.GetHostAddresses(host);
            }
            catch
            {
                return Enumerable.Empty<IPAddress>();
            }
        }
    }
}
