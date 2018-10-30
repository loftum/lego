using System.Threading.Tasks;
using Shared;

namespace ServoPwmTest
{
    class Program
    {
        static int Main(string[] args)
        {
            return ConsoleRunner.Run(() => Run(args));
        }

        private static Task Run(string[] args)
        {
            return Task.CompletedTask;
        }
    }
}
