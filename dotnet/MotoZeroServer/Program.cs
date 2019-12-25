using System;
using System.Threading;
using System.Threading.Tasks;
using Devices.ThePiHut.MotoZero;
using LCTP.Core.Server;
using Unosquare.RaspberryIO;

namespace MotoZeroServer
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("MotoZero Server v0.0");
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
            using (var board = new MotoZeroBoard(Pi.Gpio))
            {
                var controller = new MotoZeroController(board);
                using (var server = new LctpServer(5080, controller))
                {
                    await server.Start(cancellationToken);
                }
            }
        }
    }
}
