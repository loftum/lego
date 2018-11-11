using System;
using System.Threading;
using System.Threading.Tasks;
using Devices._4tronix;
using LCTP.Server;
using Unosquare.RaspberryIO;

namespace LegoCarServer
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Lego Car Server v0.0");
            using (var source = new CancellationTokenSource())
            {
                Console.WriteLine("Hello");
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
            using (var board = new PiconZeroBoard(Pi.I2C))
            {
                var controller = new LegoCarController(board, new LegoCarOptions());
                using (var server = new LctpServer(5080, controller))
                {
                    await server.Start(cancellationToken);
                }
            }
        }
    }
}
