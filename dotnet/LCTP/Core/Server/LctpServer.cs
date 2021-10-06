using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Convenient.Gooday;
using LCTP.Core.Handshake;
using LCTP.Core.Net;
using Maths.Logging;

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
        private readonly NetworkServicePublisher _publisher;

        public LctpServer(int port, IController controller)
        {
            _controller = controller;
            _port = port;
            _listener = CreateListener(port);
            _publisher = new NetworkServicePublisher(_name,
                "_legocar._tcp",
                "local",
                (ushort) port,
                new Dictionary<string, string>
                {
                    ["_d"] = _name
                },
                LogAdapter.For(nameof(NetworkServicePublisher)));
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
                _publisher.Start();
                listener.Listen(1);
                cancellationToken.Register(() =>
                {
                    _listener.Close();
                    _publisher.StopAsync().Wait(2000);
                });

                while (!cancellationToken.IsCancellationRequested)
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
            catch (SocketException)
            {
                return;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return;
            }
            finally
            {
                _logger.Trace("Waiting for publisher to stop");
                await _publisher.StopAsync();
                _logger.Trace("Done");
            }
        }

        public void Dispose()
        {
            _publisher?.Dispose();
            _listener?.Dispose();
        }
    }
}