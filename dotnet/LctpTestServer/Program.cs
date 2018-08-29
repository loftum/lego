using System;
using System.Threading;
using System.Threading.Tasks;
using LCTP;
using LCTP.Server;

namespace LctpTestServer
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("LCTP Test Server v0.0");
            using (var source = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, e) => source.Cancel();
                try
                {
                    Run(source.Token).Wait(source.Token);
                    return 0;
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Exiting ...");
                }
                catch (AggregateException e)
                {
                    Console.WriteLine(e.InnerException);
                    return -1;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return -1;
                }
                finally
                {
                    Console.WriteLine("Bye!");
                }
            }
            return 0;
        }

        private static async Task Run(CancellationToken cancellationToken)
        {
            using(var server = new LctpServer(LctpServer.DefaultPort, new EchoController()))
            {
                await server.Start(cancellationToken);
            }
        }
    }

    public class EchoController : IController
    {
        public void ConnectionClosed()
        {

        }

        public void ConnectionOpened()
        {

        }

        public Task<ResponseMessage> Execute(RequestMessage request)
        {
            return Task.FromResult(new ResponseMessage
            {
                Content = request.Content
            });
        }
    }
}
