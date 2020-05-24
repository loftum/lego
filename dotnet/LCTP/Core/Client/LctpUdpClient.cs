using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LCTP.Core.Extensions;

namespace LCTP.Core.Client
{
    public class LctpUdpClient : ILctpClient
    {
        public bool Connected { get; private set; }
        public Func<ResponseMessage, Task> OnResponseReceived { get; set; } = m => Task.CompletedTask;
        private readonly Encoding _encoding = new UTF8Encoding();
        private readonly IPEndPoint _remoteEp;
        private readonly UdpClient _client;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private Task _run;
        
        public LctpUdpClient(string host, int port)
        {
            var entry = Dns.GetHostEntry(host);
            var ipAddress = entry.AddressList.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            _remoteEp = new IPEndPoint(ipAddress, port);
            var endpoint = new IPEndPoint(IPAddress.Any, 0);
            _client = new UdpClient(endpoint);
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
            await _client.SendAsync(RequestMessage.Connect, _remoteEp);
            var response = await _client.ReceiveResponseAsync(timeout);
            if (response.Message.StatusCode != 200)
            {
                throw new Exception("Could not connect");
            }
            Connected = true;
            _run = Run();
        }

        public async Task DisconnectAsync()
        {
            if (!Connected)
            {
                return;
            }
            await _client.SendAsync(RequestMessage.Disconnect, _remoteEp);
            Connected = false;
            _run = null;
        }

        private async Task Run()
        {
            while (Connected)
            {
                var response = await _client.ReceiveResponseAsync();
                if (response.Message.Content == "DISCONNECTED")
                {
                    Connected = false;
                    return;
                }
                await OnResponseReceived?.Invoke(response.Message);
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
                await _client.SendAsync(request, _remoteEp);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        public void Dispose()
        {
        }
    }
}