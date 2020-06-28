using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LCTP.Core.Client;

namespace LCTP.Core.Server
{
    public class LctpClientHandler : ILctpClientHandler
    {
        private readonly UTF8Encoding _encoding = Encodings.Utf8NoBom;
        private readonly ICommunicator _communicator;
        private readonly IController _controller;

        public LctpClientHandler(ICommunicator communicator, IController controller)
        {
            _communicator = communicator;
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
                    await _communicator.SendAsync(ResponseMessage.Disconnected());
                    return;
                }

                var request = receive.Result;
                if (request == null)
                {
                    Console.WriteLine("Request is null. Closing connection");
                    await _communicator.DisconnectAsync();
                    return;
                }

                try
                {
                    switch (request.Method)
                    {
                        case "PING":
                            await _communicator.SendAsync(ResponseMessage.Pong);
                            break;
                        case "DISCONNECT":
                            Console.WriteLine("Client disconnected. Closing connection");
                            return;
                        default:
                            var response = await _controller.Execute(request);
                            await _communicator.SendAsync(response);
                            break;
                    }
                }
                catch (ObjectDisposedException)
                {
                    return;
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
            var data = await _communicator.ReceiveAsync();
            var line = _encoding.GetString(data);
            return RequestMessage.Parse(line);
        }

        public void Dispose()
        {
            _controller.ConnectionClosed();
            _communicator.Dispose();
        }
    }

    public static class CommunicatorExtensions
    {
        public static Task SendAsync(this ICommunicator client, IMessage request)
        {
            var datagram = Encodings.Utf8NoBom.GetBytes(request.Format());
            return client.SendAsync(datagram);
        }
    }
}