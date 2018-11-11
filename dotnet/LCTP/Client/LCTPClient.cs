using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using LCTP.Extensions;

namespace LCTP.Client
{
    public static class SocketExtensions
    {
        public static void Connect(this Socket socket, EndPoint endpoint, TimeSpan timeout)
        {
            var result = socket.BeginConnect(endpoint, null, null);
            if (result.AsyncWaitHandle.WaitOne(timeout, true))
            {
                socket.EndConnect(result);
                return;
            }
            socket.Close();
            throw new SocketException((int)SocketError.TimedOut);
        }
    }

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

        public async Task<ResponseMessage> Send(RequestMessage request)
        {
            await _writer.WriteLineAndFlushAsync(request.Format());
            var response = await _reader.ReadLineAsync();
            return ResponseMessage.Parse(response);
        }

        public async Task<ResponseMessage> Ping()
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
        public static Task<ResponseMessage> Set(this LctpClient client, string path, string content)
        {
            return client.Send(new RequestMessage
            {
                Method = "SET",
                Path = path,
                Content = content
            });
        }

        public static Task<ResponseMessage> Get(this LctpClient client, string path)
        {
            return client.Send(new RequestMessage
            {
                Method = "GET",
                Path = path,
            });
        }
    }
}