using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LCTP.Core.Extensions;

namespace LCTP.Core.Client
{
    /// <summary>
    /// Lego Command Transfer Protocol client
    /// </summary>
    public class LctpClient : IDisposable
    {
        public bool Connected => _socket.Connected;
        private readonly Encoding _encoding = new UTF8Encoding();
        private readonly EndPoint _remoteEp;
        private readonly Socket _socket;
        private StreamReader _reader;
        private StreamWriter _writer;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public LctpClient(string host, int port)
        {
            var entry = Dns.GetHostEntry(host);
            var ipAddress = entry.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            _remoteEp = new IPEndPoint(ipAddress, port);
            _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = 1000,
                ReceiveTimeout = 1000
            };
        }

        public void Connect()
        {
            Connect(TimeSpan.FromSeconds(5));
        }

        public void Connect(TimeSpan timeout)
        {
            if (Connected)
            {
                return;
            }
            _socket.Connect(_remoteEp, timeout);
            var stream = new NetworkStream(_socket);
            _reader = new StreamReader(stream);
            _writer = new StreamWriter(stream, _encoding);
        }

        public void Disconnect()
        {
            if (!Connected)
            {
                return;
            }
            
            _writer.Close();
            _writer.Dispose();
            _writer = null;
            _reader.Close();
            _reader.Dispose();
            _reader = null;
            _socket.Disconnect(false);
        }

        public async Task<ResponseMessage> SendAsync(RequestMessage request)
        {
            await _semaphore.WaitAsync();
            try
            {
                await _writer?.WriteLineAndFlushAsync(request.Format());
                var response = await _reader?.ReadLineAsync();
                return ResponseMessage.Parse(response);
            }
            finally
            {
                _semaphore.Release();
            }
            
        }

        public async Task<ResponseMessage> PingAsync()
        {
            var message = new RequestMessage
            {
                Method = "PING"
            };
            await _writer.WriteLineAndFlushAsync(message.Format());
            var response = await _reader.ReadLineAsync();
            return ResponseMessage.Parse(response);
        }

        public void Dispose()
        {
            _socket?.Dispose();
            _reader?.Dispose();
            _writer?.Dispose();
        }
    }

    public static class LctpClientExtensions
    {
        public static Task<ResponseMessage> SetAsync(this LctpClient client, string path, string content)
        {
            return client.SendAsync(new RequestMessage
            {
                Method = "SET",
                Path = path,
                Content = content
            });
        }

        public static Task<ResponseMessage> GetAsync(this LctpClient client, string path)
        {
            return client.SendAsync(new RequestMessage
            {
                Method = "GET",
                Path = path,
            });
        }
    }
}