using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MouseTest
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                Console.WriteLine("MouseTest");
                Console.WriteLine();
                using (var source = new CancellationTokenSource())
                {
                    Console.CancelKeyPress += (s, e) => source.Cancel();
                    await RunAsync(source.Token);
                    return 0;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }
        }

        private static async Task RunAsync(CancellationToken cancellationToken)
        {
            using (var stream = File.OpenRead("/dev/input/mice"))
            {
                var buffer = new byte[3];
                while (!cancellationToken.IsCancellationRequested)
                {
                    var read = await stream.ReadAsync(buffer, 0, 3, cancellationToken);
                    if (read < 0)
                    {
                        return;
                    }

                    var button = buffer[0];
                    var buttonLeft = button & 0b0000_0001;
                    var buttonMiddle = (button & 0b0000_0100) >> 2;
                    var buttonRight = (button & 0b0000_0010) >> 1;
                    var x = (sbyte) buffer[1];
                    var y = (sbyte) buffer[2];
                    
                    Console.WriteLine(string.Join(" ", buffer));
                    Console.WriteLine($"left:{buttonLeft}, right:{buttonRight}, middle:{buttonMiddle}, x:{x}, y:{y}");
                }
            }
        }
    }
}