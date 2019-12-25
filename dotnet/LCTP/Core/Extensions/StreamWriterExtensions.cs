using System.IO;
using System.Threading.Tasks;

namespace LCTP.Core.Extensions
{
    public static class StreamWriterExtensions
    {
        public static async Task WriteLineAndFlushAsync(this StreamWriter writer, string line)
        {
            await writer.WriteLineAsync(line);
            await writer.FlushAsync();
        }
    }
}