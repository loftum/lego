using System.Net;
using System.Net.Sockets;

namespace LCTP.Core.Handshake
{
    public static class UdpClientExtensions
    {
        public static IPEndPoint GetLocalIpEndpoint(this UdpClient client) => client.Client.GetLocalIpEndpoint();
        public static IPEndPoint GetLocalIpEndpoint(this Socket socket) => (IPEndPoint) socket.LocalEndPoint;

        public static IPEndPoint GetRemoteIpEndpoint(this UdpClient client) => client.Client.GetRemoteIpEndpoint();
        public static IPEndPoint GetRemoteIpEndpoint(this Socket socket) => (IPEndPoint) socket.RemoteEndPoint;
    }
}