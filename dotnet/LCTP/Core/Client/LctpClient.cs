using System;
using System.Threading;
using System.Threading.Tasks;
using LCTP.Core.Handshake;

namespace LCTP.Core.Client
{
    public class LctpClient : ILctpClient
    {
        private readonly string _name;
        public Func<ResponseMessage, Task> OnResponseReceived { get; set; } = r => Task.CompletedTask;

        public bool Connected => _communicator?.IsConnected ?? false;
        
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly string _host;
        private readonly int _port;
        private ICommunicator _communicator;
        private Task _run;
        
        public LctpClient(string name, string host, int port)
        {
            _name = name;
            _host = host;
            _port = port;
        }
        
        public async Task ConnectAsync()
        {
            var handshake = new ClientHandshake(_name);
            var communicator = await handshake.ConnectAsync(_host, _port);
            if (communicator == null)
            {
                return;
            }

            _communicator = communicator;
            _run = Run();
        }
        
        private async Task Run()
        {
            while (Connected)
            {
                var bytes = await _communicator.ReceiveAsync();
                var s = Encodings.Utf8NoBom.GetString(bytes);
                var message = ResponseMessage.Parse(s);
                await OnResponseReceived?.Invoke(message);
            }
        }

        public Task DisconnectAsync()
        {
            Console.WriteLine("LCTP client disconnecting");
            return _communicator?.DisconnectAsync() ?? Task.CompletedTask;
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
                var datagram = Encodings.Utf8NoBom.GetBytes(request.Format());
                await _communicator.SendAsync(datagram);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        public void Dispose()
        {
            _communicator?.Dispose();
        }
    }
}