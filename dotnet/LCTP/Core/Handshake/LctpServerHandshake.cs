using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Convenient.Gooday.Net;
using LCTP.Core.Client;
using LCTP.Core.Extensions;
using LCTP.Core.Server;
using LCTP.Logging;

namespace LCTP.Core.Handshake
{
    public class LctpServerHandshake
    {
        private readonly ILogger _logger = Log.For<LctpServerHandshake>();
        private readonly string _name;
        private readonly IController _controller;

        public LctpServerHandshake(string name, IController controller)
        {
            _name = name;
            _controller = controller;
        }

        public async Task<ILctpClientHandler> ExecuteAsync(Socket socket)
        {
            try
            {
                var stream = new NetworkStream(socket);
                var reader = new StreamReader(stream);
                var writer = new StreamWriter(stream);
            
                var clientHello = await reader.ReadAsync(HelloMessage.Decode);
                if (clientHello == null)
                {
                    _logger.Error("Client hello is null");
                    return null;
                }
                _logger.Debug($"Client hello: {clientHello.Name}");

                var serverHello = new HelloMessage
                {
                    Name = _name
                };
                _logger.Debug($"Server hello: {_name}");
                await writer.WriteLineAndFlushAsync(serverHello.Encode());

                var command = await reader.ReadAsync(CommandMessage.Decode);
                if (command == null)
                {
                    _logger.Error("Command message is null");
                    return null;
                }

                switch (command.Command)
                {
                    case "car":
                        return await CreateCarClientHandler(socket, reader, writer);
                    default:
                        return null;
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
                socket.Disconnect(true);
                socket.Close();
                socket.Dispose();
                return null;
            }
        }

        private async Task<ILctpClientHandler> CreateCarClientHandler(Socket socket, StreamReader reader, StreamWriter writer)
        {
            _logger.Trace("Creating car handler");
            var udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 0));

            var networkInterfaces = Network.GetUsableInterfaces().ToList();
            foreach (var i in networkInterfaces)
            {
                _logger.Debug($"Interface: {i.Index}: {i.Interface.NetworkInterfaceType} desc:{i.Interface.Description}");
            }
             
            var serverConnect = new ConnectMessage
            {
                Port = udpClient.GetLocalIpEndpoint().Port
            }.Encode();
            _logger.Debug($"Server connect: {serverConnect}");
            await writer.WriteLineAndFlushAsync(serverConnect);
            
            
            var clientConnect = await reader.ReadAsync(ConnectMessage.Decode);
            if (clientConnect == null)
            {
                _logger.Error("Got null client connect");
                return null;
            }
            _logger.Trace($"Client connect: {clientConnect.Encode()}");
            
            var remoteEndpoint = new IPEndPoint(socket.GetRemoteIpEndpoint().Address, clientConnect.Port);
            _logger.Trace($"Client endpoint: {remoteEndpoint}");
            var communicator = new Communicator(socket, reader, writer, udpClient, remoteEndpoint);
            
            return new LctpClientHandler(communicator, _controller);
        }
    }
}