using System.Threading.Tasks;

namespace LCTP.Core.Client
{
    public static class LctpClientExtensions
    {
        public static Task SetAsync(this ILctpClient client, string path, string content)
        {
            return client.SendAsync(new RequestMessage
            {
                Method = "SET",
                Path = path,
                Content = content
            });
        }

        public static Task GetAsync(this ILctpClient client, string path)
        {
            return client.SendAsync(new RequestMessage
            {
                Method = "GET",
                Path = path,
            });
        }

        public static Task PingAsync(this ILctpClient client)
        {
            return client.SendAsync(new RequestMessage
            {
                Method = "PING"
            });   
        }
    }
}