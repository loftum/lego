using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shared
{
    public class ConsoleRunner
    {
        public static int Run(Func<CancellationToken, Task> task)
        {
            using (var source = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, a) => source.Cancel();
                try
                {
                    task(source.Token).Wait(source.Token);
                    return 0;
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e);
                    Console.ResetColor();
                    return -1;
                }
                finally
                {
                    Console.WriteLine("Bye!");
                }
            }
        }
    }
}
