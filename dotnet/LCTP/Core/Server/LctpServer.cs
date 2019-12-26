using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LCTP.Core.Net;

namespace LCTP.Core.Server
{
    /// <summary>
    /// Lego Command Transfer Protocol Server
    /// </summary>
    public class LctpServer: IDisposable
    {
        public const int DefaultPort = 5080;

        private readonly IController _controller;
        private readonly Socket _listener;
        private readonly int _port;

        public LctpServer(int port, IController controller)
        {
            _controller = controller;
            _port = port;
            _listener = CreateListener(port);
        }

        private static Socket CreateListener(int port)
        {
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var hostname = Dns.GetHostName();
            Console.WriteLine($"hostname: {hostname}");

            foreach (var address in HackyDns.GetLocalAddresses()
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                .Distinct())
            {
                Console.WriteLine($"Address: {address}");
            }
            listener.Bind(new IPEndPoint(IPAddress.Any, port));

            return listener;
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            try
            {
                _listener.Listen(1);
                
                while (true)
                {
                    Console.WriteLine($"Listening for connections on port {_port}");
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
