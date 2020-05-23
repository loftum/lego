using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LCTP;
using LCTP.Core;
using LCTP.Core.Client;
using LCTP.Core.Server;

namespace MotoZeroClient
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintUsage();
                return 0;
            }
            var hostAndPort = args[0].Split(':');
            var host = hostAndPort[0];
            var port = args.Length > 1 && int.TryParse(hostAndPort[1], out var p)
                ? p
                : LctpTcpServer.DefaultPort;

            using (var source = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, e) => source.Cancel();
                try
                {
                    Run(host, port, source.Token).Wait(source.Token);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return -1;
                }
            }
            Console.WriteLine("Bye!");
            return 0;
        }

        private static async Task Run(string host, int port, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Connecting to {host}:{port}");
            using (var client = new LctpClient(host, port))
            {
                while (true)
                {
                    Console.Write("> ");
                    var line = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    var parts = line.Split(' ');
                    if (parts.Length < 2)
                    {
                        Console.WriteLine("Expected <method> <path> [content]");
                        continue;
                    }
                    var response = await client.SendAsync(new RequestMessage
                    {
                        Method = parts[0],
                        Path = parts[1],
                        Content = string.Join(" ", parts.Skip(2))
                    });
                    Console.WriteLine("Response:");
                    Console.WriteLine(response);
                }
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine($"{Process.GetCurrentProcess().ProcessName} <host>[:port]");
        }
    }
}
