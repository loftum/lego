using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Convenient.Gooday.Net;
using LCTP.Core.Client;
using LCTP.Core.Extensions;
using LCTP.Logging;

namespace LCTP.Core.Handshake
{
    public class ClientHandshake
    {
        private readonly ILogger _logger = Log.For<ClientHandshake>();
        private readonly string _name;

        public ClientHandshake(string name)
        {
            _name = name;
        }

        public async Task<ICommunicator> ConnectAsync(string host, int port)
        {
            var entry = await Dns.GetHostEntryAsync(host);
            var ipAddress = entry.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = 1000,
                ReceiveTimeout = 1000
            };
            var remoteEp = new IPEndPoint(ipAddress, port);
            
            await socket.ConnectAsync(remoteEp, TimeSpan.FromSeconds(5));
            var stream = new NetworkStream(socket);
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream);
            
            var clientHello = new HelloMessage
            {
                Name = _name
            }.Encode();
            _logger.Trace($"Client hello: {clientHello}");
            await writer.WriteLineAndFlushAsync(clientHello);

            var serverHello = await reader.ReadAsync(HelloMessage.Decode);
            if (serverHello == null)
            {
                _logger.Error("Server hello is null");
                return null;
            }

            var command = new CommandMessage
            {
                Command = "car"
            }.Encode();
            
            _logger.Trace($"Client command: {command}");
            await writer.WriteLineAndFlushAsync(command);

            var serverConnect = await reader.ReadAsync(ConnectMessage.Decode);
            if (serverConnect == null)
            {
                _logger.Error("Server connect is null");
                return null;
            }
            _logger.Trace($"Server connect: {serverConnect.Encode()}");
            
            

            var remoteEndpoint = new IPEndPoint(ipAddress, serverConnect.Port);
            var udpClient = new UdpClient(new IPEndPoint(IPAddress.Any,0));
            
            
            var networkInterfaces = Network.GetUsableInterfaces().ToList();
            foreach (var i in networkInterfaces)
            {
                _logger.Debug($"Interface: {i.Index}: {i.Interface.NetworkInterfaceType} desc:{i.Interface.Description}");
            }
            var clientConnect = new ConnectMessage
            {
                Port = udpClient.GetLocalIpEndpoint().Port
            }.Encode();
            _logger.Trace($"Client connect: {clientConnect}");
            await writer.WriteLineAndFlushAsync(clientConnect);
            _logger.Trace($"Server endpoint: {remoteEndpoint}");
            return new Communicator(socket, reader, writer, udpClient, remoteEndpoint);
        }
    }
}