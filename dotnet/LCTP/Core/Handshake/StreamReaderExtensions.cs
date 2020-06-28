using System;
using System.IO;
using System.Threading.Tasks;

namespace LCTP.Core.Handshake
{
    static class StreamReaderExtensions
    {
        public static async Task<T> ReadAsync<T>(this StreamReader reader, Func<string, T> decode)
        {
            var line = await reader.ReadLineAsync();
            return decode(line);
        }
    }
}