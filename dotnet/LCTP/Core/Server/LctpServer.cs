using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using LCTP.Core.Handshake;
using LCTP.Core.Net;
using LCTP.Logging;

namespace LCTP.Core.Server
{
    public class LctpServer : ILctpServer
    {
        private readonly ILogger _logger = Log.For<LctpServer>();
        private readonly string _name = Environment.MachineName;
        public const int DefaultPort = 5080;

        private readonly IController _controller;
        private readonly Socket _listener;
        private readonly int _port;
        private readonly LctpServerHandshake _handshake;

        public LctpServer(int port, IController controller)
        {
            _controller = controller;
            _port = port;
            _listener = CreateListener(port);
            _handshake = new LctpServerHandshake(_name, controller);
        }
        
        private Socket CreateListener(int port)
        {
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var hostname = Dns.GetHostName();
            _logger.Info($"hostname: {hostname}");

            foreach (var address in HackyDns.GetLocalAddresses()
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                .Distinct())
            {
                _logger.Info($"Address: {address}");
            }
            listener.Bind(new IPEndPoint(IPAddress.Any, port));

            return listener;
        }
        
        public Task RunAsync(CancellationToken cancellationToken)
        {
            return Listen(_listener, cancellationToken);
        }

        private async Task Listen(Socket listener, CancellationToken cancellationToken)
        {
            try
            {
                listener.Listen(1);

                while (true)
                {
                    _logger.Info($"Listening for connections on port {_port}");
                    using (var socket = await _listener.AcceptAsync())
                    {
                        _logger.Info($"Got connection from {socket.RemoteEndPoint}");
                        try
                        {
                            using (var handler = await _handshake.ExecuteAsync(socket))
                            {
                                await handler.Handle(cancellationToken);    
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return;
            }
        }

        public void Dispose()
        {
            _listener?.Dispose();
        }
    }
}