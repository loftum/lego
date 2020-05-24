using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LCTP.Core.Extensions;

namespace LCTP.Core.Server
{
    public class LctpUdpServer : ILctpServer
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
                    var data = await _client.ReceiveRequestAsync();
                    if (data.Request.Method != "CONNECT")
                    {
                        continue;
                    }

                    await _client.SendAsync(ResponseMessage.Ok(), data.RemoteEndPoint);
                    using var handler = new LctpUdpClientHandler(_client, data.RemoteEndPoint, _controller);
                    await handler.Handle(cancellationToken);
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