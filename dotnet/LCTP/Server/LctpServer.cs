using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LCTP.Server
{
    /// <summary>
    /// Lego Command Transfer Protocol Server
    /// </summary>
    public class LctpServer: IDisposable
    {
        public const int DefaultPort = 5080;

        private readonly IController _controller;
        private readonly IPEndPoint _localEndpoint;
        private readonly Socket _listener;

        public LctpServer(int port, IController controller)
        {
            _controller = controller;

            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList[0];
            _localEndpoint = new IPEndPoint(ipAddress, port);
            _listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            try
            {
                _listener.Bind(_localEndpoint);
                _listener.Listen(1);
                
                while (true)
                {
                    Console.WriteLine($"Listening for connections on port {_localEndpoint.Port}");
                    var socket = await _listener.AcceptAsync();
                    Console.WriteLine($"Got connection from {socket.RemoteEndPoint}");
                    var handler = new ClientHandler(socket, _controller);
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
            _listener?.Dispose();
        }
    }
}
