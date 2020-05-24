using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LCTP.Core.Extensions;

namespace LCTP.Core.Server
{
    public class LctpUdpClientHandler : ILctpClientHandler
    {
        private readonly Encoding _encoding = new UTF8Encoding();
        private readonly IController _controller;
        private readonly UdpClient _client;
        private readonly IPEndPoint _remoteEndpoint;

        public LctpUdpClientHandler(UdpClient client, IPEndPoint remoteEndpoint, IController controller)
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
                    await _client.SendAsync(ResponseMessage.Disconnected(), _remoteEndpoint);
                    return;
                }

                var request = receive.Result;
                if (request == null)
                {
                    Console.WriteLine("Request is null. Closing connection");
                    await _client.SendAsync(ResponseMessage.Disconnected(), _remoteEndpoint);
                    return;
                }

                try
                {
                    switch (request.Method)
                    {
                        case "PING":
                            await _client.SendAsync(ResponseMessage.Pong, _remoteEndpoint);
                            break;
                        case "DISCONNECT":
                            Console.WriteLine("Client disconnected. Closing connection");
                            return;
                        default:
                            var response = await _controller.Execute(request);
                            await _client.SendAsync(response, _remoteEndpoint);
                            break;
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
            while (!_remoteEndpoint.Equals(data.RemoteEndPoint))
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