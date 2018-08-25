using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using LCTP.Extensions;

namespace LCTP.Client
{
    /// <summary>
    /// Lego Command Transfer Protocol client
    /// </summary>
    public class LctpClient: IDisposable
    {
        private readonly Encoding _encoding = new UTF8Encoding();
        private readonly IPEndPoint _remoteEp;
        private readonly Socket _socket;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;

        public LctpClient(string host, int port)
        {
            var ipAddress = IPAddress.Parse(host);
            _remoteEp = new IPEndPoint(ipAddress, port);
            _socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            _socket.Connect(_remoteEp);
            var stream = new NetworkStream(_socket);
            _reader = new StreamReader(stream);
            _writer = new StreamWriter(stream, _encoding);
        }

        public async Task<ResponseMessage> Send(RequestMessage request)
        {
            await _writer.WriteLineAndFlushAsync(request.Format());
            var response = await _reader.ReadLineAsync();
            return ResponseMessage.Parse(response);
        }

        public void Dispose()
        {
            _reader?.Dispose();
            _writer?.Dispose();
        }
    }
}