using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LCTP.Core.Extensions
{
    public static class UdpClientExtensions
    {
        private static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);
        
        public static Task<int> SendAsync(this UdpClient client, IMessage request, IPEndPoint endPoint)
        {
            var datagram = Utf8NoBom.GetBytes(request.Format());
            return client.SendAsync(datagram, datagram.Length, endPoint);
        }
        
        public static int Send(this UdpClient client, IMessage request, IPEndPoint endPoint)
        {
            var datagram = Utf8NoBom.GetBytes(request.Format());
            return client.Send(datagram, datagram.Length, endPoint);
        }

        public static async Task<UdpReceiveRequest> ReceiveRequestAsync(this UdpClient client)
        {
            var data = await client.ReceiveAsync();
            var s = Utf8NoBom.GetString(data.Buffer);
            return new UdpReceiveRequest(RequestMessage.Parse(s), data.RemoteEndPoint);
        }
        
        public static async Task<UdpReceiveResponse> ReceiveResponseAsync(this UdpClient client)
        {
            var data = await client.ReceiveAsync();
            var s = Utf8NoBom.GetString(data.Buffer);
            return new UdpReceiveResponse(ResponseMessage.Parse(s), data.RemoteEndPoint);
        }
    }

    public struct UdpReceiveRequest
    {
        public RequestMessage Request { get; }
        public IPEndPoint RemoteEndPoint { get; }
        
        public UdpReceiveRequest(RequestMessage request, IPEndPoint remoteEndPoint)
        {
            Request = request;
            RemoteEndPoint = remoteEndPoint;
        }
    }
    
    public struct UdpReceiveResponse
    {
        public ResponseMessage Message { get; }
        public IPEndPoint RemoteEndPoint { get; }
        
        public UdpReceiveResponse(ResponseMessage message, IPEndPoint remoteEndPoint)
        {
            Message = message;
            RemoteEndPoint = remoteEndPoint;
        }
    }
}