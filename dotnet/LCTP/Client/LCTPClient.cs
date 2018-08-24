using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LCTP.Client
{
    /// <summary>
    /// Lego Command Transfer Protocol client
    /// </summary>
    public class LctpClient: IDisposable
    {
        private readonly Encoding _encoding = new UTF8Encoding();
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;

        public LctpClient(string host, int port)
        {
            var ipHostInfo = Dns.GetHostEntry(host);
            var ipAddress = ipHostInfo.AddressList[0];
            var remoteEp = new IPEndPoint(ipAddress, port);
            var socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(remoteEp);
            var stream = new NetworkStream(socket);
            _reader = new StreamReader(stream);
            _writer = new StreamWriter(stream, _encoding);
        }

        public async Task<ResponseMessage> Send(RequestMessage request)
        {
            await _writer.WriteLineAsync(request.Format());
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