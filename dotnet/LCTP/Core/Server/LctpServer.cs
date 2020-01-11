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
        private readonly Socket _spectatorListener;
        private readonly int _spectatorPort;

        public LctpServer(int port, IController controller)
        {
            _controller = controller;
            _port = port;
            _listener = CreateListener(port);
        }
        
        public LctpServer(int port, int spectatorPort, IController controller)
        {
            _controller = controller;
            _port = port;
            _spectatorPort = spectatorPort;
            _listener = CreateListener(port);
            _spectatorListener = CreateListener(spectatorPort);
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

        public Task RunAsync(CancellationToken cancellationToken)
        {
            var listeners = new[] {_listener, _spectatorListener}.Where(l => l != null).Select(l => Listen(l, cancellationToken));
            return Task.WhenAll(listeners);
        }

        private async Task Listen(Socket listener, CancellationToken cancellationToken)
        {
            try
            {
                listener.Listen(1);

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
