using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LCTP.Core.Server
{
    public class UdpClientHandler : IDisposable
    {
        private readonly Encoding _encoding = new UTF8Encoding();
        private readonly IController _controller;
        private readonly UdpClient _client;
        private readonly IPEndPoint _remoteEndpoint;

        public UdpClientHandler(UdpClient client, IPEndPoint remoteEndpoint, IController controller)
        {
            _client = client;
            _remoteEndpoint = remoteEndpoint;
            _controller = controller;
        }
        
        public async Task Handle(CancellationToken cancellationToken)
        {
            try
            {
                _controller.ConnectionOpened();
                await DoHandle(cancellationToken);
            }
            catch (Exception e)
            {
                _controller.ConnectionClosed();
                Console.WriteLine(e);
            }
            _controller.ConnectionClosed();
        }

        private async Task DoHandle(CancellationToken cancellationToken)
        {
            const int timeout = 1000;
            while (true)
            {
                var receive = Receive(cancellationToken);
                if (await Task.WhenAny(receive, Task.Delay(timeout, cancellationToken)) != receive)
                {
                    Console.WriteLine($"Client has not sent any data for {timeout} ms. Closing connection");
                    return;
                }

                var request = receive.Result;
                if (request == null)
                {
                    Console.WriteLine("Request is null. Closing connection");
                    return;
                }

                try
                {
                    if (request.Method == "PING")
                    {
                        var bytes = _encoding.GetBytes(ResponseMessage.Pong.Format());
                        await _client.SendAsync(bytes, bytes.Length, _remoteEndpoint);
                    }
                    else
                    {
                        var response = await _controller.Execute(request);
                        var bytes = _encoding.GetBytes(response.Format());
                        await _client.SendAsync(bytes, bytes.Length, _remoteEndpoint);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        private async Task<RequestMessage> Receive(CancellationToken cancellationToken)
        {
            var data = await _client.ReceiveAsync();
            while (data.RemoteEndPoint != _remoteEndpoint)
            {
                data = await _client.ReceiveAsync();
            }
            var line = _encoding.GetString(data.Buffer);
            return RequestMessage.Parse(line);
        }

        public void Dispose()
        {
        }
    }
}