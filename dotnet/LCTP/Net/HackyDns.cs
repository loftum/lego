using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LCTP.Net
{
    public static class HackyDns
    {
        public static IEnumerable<IPAddress> GetHostAddresses(string host)
        {
            return Dns.GetHostAddresses(host)
                      .Concat(TryGetHostAddresses($"{host}.local"));
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
