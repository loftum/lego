using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shared
{
    public static class ConsoleRunner
    {
        public static async Task<int> RunAsync(Func<CancellationToken, Task> task)
        {
            using var source = new CancellationTokenSource();
            Console.CancelKeyPress += (s, a) =>
            {
                source.Cancel();
                a.Cancel = true;
            };
            try
            {
                await task(source.Token);
                return 0;
            }
            catch (TaskCanceledException)
            {
                return 0;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Print(e);
                Console.ResetColor();
                return -1;
            }
            finally
            {
                Console.WriteLine("Bye!");
            }
        }

        private static void Print(Exception e)
        {
            var text = new StringBuilder()
                .AppendLine(e.GetType().Name)
                .AppendLine(e.Message)
                .AppendLine(e.StackTrace);
            Console.WriteLine(text.ToString());
        }
    }
}
