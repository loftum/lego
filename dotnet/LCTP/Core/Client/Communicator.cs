using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using LCTP.Core.Extensions;
using Maths.Logging;

namespace LCTP.Core.Client
{
    public interface ICommunicator : IDisposable
    {
        bool IsConnected { get; }
        Task SendAsync(byte[] message);
        Task<byte[]> ReceiveAsync();
        Task DisconnectAsync();
    }
    
    public class Communicator : ICommunicator
    {
        private readonly ILogger _logger = Log.For<Communicator>();
        public bool IsConnected => _socket.Connected;
        public IPEndPoint LocalEndpoint { get; }
        public IPEndPoint RemoteEndoint { get; }
        private readonly UdpClient _udpClient;
        private readonly Socket _socket;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private Task _readTask;

        public Communicator(Socket socket, StreamReader reader, StreamWriter writer, UdpClient udpClient, EndPoint remoteEp)
        {
            _socket = socket;
            _reader = reader;
            _writer = writer;
            _udpClient = udpClient;
            LocalEndpoint = (IPEndPoint) udpClient.Client.LocalEndPoint;
            RemoteEndoint = (IPEndPoint) remoteEp;
            _readTask = ReceiveTcpAsync();
        }


        public async Task SendAsync(byte[] message)
        {
            await _udpClient.SendAsync(message, message.Length, RemoteEndoint);
        }

        public async Task<byte[]> ReceiveAsync()
        {
            var data = await _udpClient.ReceiveAsync();
            while (!RemoteEndoint.Equals(data.RemoteEndPoint))
            {
                _logger.Debug($"Wrong endpoint: {data.RemoteEndPoint}");
                data = await _udpClient.ReceiveAsync();
            }
            return data.Buffer;
        }

        public async Task DisconnectAsync()
        {
            try
            {
                await _writer.WriteLineAndFlushAsync("DISCONNECT");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task ReceiveTcpAsync()
        {
            try
            {
                while (true)
                {
                    var message = await _reader.ReadLineAsync();
                    switch (message)
                    {
                        case "PING":
                            await _writer.WriteLineAndFlushAsync("PONG");
                            break;
                        case "PONG":
                            break;
                        case "DISCONNECT":
                            Close();
                            break;
                        default:
                            break;
                    }    
                }
            }
            catch
            {
                Close();
            }
        }

        private void Close()
        {
            try
            {
                _socket.Disconnect(true);
                _reader.Close();
                _writer.Close();
                _socket.Close();
                _udpClient.Close();
            }
            catch
            {
                
            }
        }

        public void Dispose()
        {
            Close();
            _socket.Dispose();
            _reader.Dispose();
            _writer.Dispose();
            _udpClient.Dispose();
        }
    }
}