﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LCTP.Core.Extensions;

namespace LCTP.Core.Server
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
            try
            {
                _controller.ConnectionOpened();
                await DoHandle(cancellationToken);
            }
            catch (IOException e)
            {
                _controller.ConnectionClosed();
                Console.WriteLine(e.Message);
            }
            catch (SocketException e)
            {
                _controller.ConnectionClosed();
                Console.WriteLine(e.Message);
            }
            _controller.ConnectionClosed();
        }

        private async Task DoHandle(CancellationToken cancellationToken)
        {
            const int timeout = 1000;
            var sw = new Stopwatch();
            while (true)
            {
                sw.Reset();
                sw.Start();
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
                        await _writer.WriteLineAndFlushAsync(ResponseMessage.Pong.Format());
                    }
                    else
                    {
                        var response = await _controller.Execute(request);
                        await _writer.WriteLineAndFlushAsync(response.Format());    
                    }
                }
                catch (Exception e)
                {
                    await _writer.WriteLineAndFlushAsync(new ResponseMessage
                    {
                        StatusCode = 500,
                        Content = e.Message
                    }.Format());
                }
                finally
                {
                    sw.Stop();
                    Console.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}ms");
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