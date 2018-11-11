using System;
using System.Linq;
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


            var hostname = Dns.GetHostName();
            Console.WriteLine($"hostname: {hostname}");

            foreach(var address in Dns.GetHostAddresses("localhost").Select(a => a.ToString()).Distinct())
            {
                Console.WriteLine($"Address: {address}");
            }
            var ipHostInfo = Dns.GetHostEntry(hostname);
            var ipAddress = ipHostInfo.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
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
                    using (var socket = await _listener.AcceptAsync())
                    {
                        Console.WriteLine($"Got connection from {socket.RemoteEndPoint}");
                        using (var handler = new ClientHandler(socket, _controller))
                        {
                            await handler.Handle(cancellationToken);
                        }
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
            _listener?.Dispose();
        }
    }
}
