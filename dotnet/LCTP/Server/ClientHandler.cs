using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LCTP.Server
{
    public class ClientHandler: IDisposable
    {
        private readonly Encoding _encoding = new UTF8Encoding();
        private readonly Socket _socket;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;
        private readonly IController _controller;

        public ClientHandler(Socket socket, IController controller)
        {
            _socket = socket;
            _controller = controller;
            var stream = new NetworkStream(socket);
            _reader = new StreamReader(stream, _encoding);
            _writer = new StreamWriter(stream, _encoding);
        }

        public async Task Handle(CancellationToken cancellationToken)
        {
            while (true)
            {
                var request = await Receive(cancellationToken);
                try
                {
                    var response = await _controller.Execute(request);
                    await _writer.WriteLineAsync(response.Format());
                }
                catch (Exception e)
                {
                    await _writer.WriteLineAsync(new ResponseMessage
                    {
                        StatusCode = 500,
                        Content = e.Message
                    }.Format());
                }
            }
        }

        private async Task<RequestMessage> Receive(CancellationToken cancellationToken)
        {
            var line = await _reader.ReadLineAsync();
            return RequestMessage.Parse(line);
        }

        public void Dispose()
        {
            _reader.Dispose();
            _writer.Dispose();
        }
    }
}