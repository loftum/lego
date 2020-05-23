using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LCTP.Core.Server
{
    public class LctpUdpServer : IDisposable
    {
        public const int DefaultPort = 5081;
        private readonly IController _controller;
        private readonly int _port;
        private readonly UdpClient _client;
        

        public LctpUdpServer(int port, IController controller)
        {
            _port = port;
            _controller = controller;
            var endpoint = new IPEndPoint(IPAddress.Any, port);
            _client = new UdpClient(endpoint);
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (true)
                {
                    Console.WriteLine($"Listening for connections on port {_port}");
                    var data = await _client.ReceiveAsync();
                    using (var handler = new UdpClientHandler(_client, data.RemoteEndPoint, _controller))
                    {
                        await handler.Handle(cancellationToken);
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}