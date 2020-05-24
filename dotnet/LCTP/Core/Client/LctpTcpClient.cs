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
    public class LctpTcpClient : ILctpClient
    {
        public bool Connected => _socket.Connected;
        public Func<ResponseMessage, Task> OnResponseReceived { get; set; } = r => Task.CompletedTask;
        private readonly Encoding _encoding = new UTF8Encoding();
        private readonly EndPoint _remoteEp;
        private readonly Socket _socket;
        private StreamReader _reader;
        private StreamWriter _writer;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private Task _run;

        public LctpTcpClient(string host, int port)
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

        public Task ConnectAsync()
        {
            return ConnectAsync(TimeSpan.FromSeconds(5));
        }

        public async Task ConnectAsync(TimeSpan timeout)
        {
            if (Connected)
            {
                return;
            }

            await _socket.ConnectAsync(_remoteEp, timeout);
            var stream = new NetworkStream(_socket);
            _reader = new StreamReader(stream);
            _writer = new StreamWriter(stream, _encoding);
            _run = Run();
        }

        public async Task DisconnectAsync()
        {
            if (!Connected)
            {
                return;
            }

            _run = null;
                 
            _writer.Close();
            _writer.Dispose();
            _writer = null;
            _reader.Close();
            _reader.Dispose();
            _reader = null;
            _socket.Disconnect(false);
            _socket.Close();
        }

        private async Task Run()
        {
            while (Connected)
            {
                try
                {
                    var line = await _reader.ReadLineAsync();
                    var response = ResponseMessage.Parse(line);
                    if (response.Content == "DISCONNECTED")
                    {
                        return;
                    }

                    await OnResponseReceived?.Invoke(response);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        public async Task SendAsync(RequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            await _semaphore.WaitAsync();
            try
            {
                await _writer?.WriteLineAndFlushAsync(request.Format());
            }
            finally
            {
                _semaphore.Release();
            }
            
        }

        public void Dispose()
        {
            _socket?.Dispose();
            _reader?.Dispose();
            _writer?.Dispose();
        }
    }
}