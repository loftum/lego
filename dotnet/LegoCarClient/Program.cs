using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LCTP;
using LCTP.Client;
using LCTP.Server;

namespace LegoCarClient
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("Lego Car Client v0.0");
            if (args.Length < 1)
            {
                PrintUsage();
                return 0;
            }
            var hostAndPort = args[0].Split(':');
            var host = hostAndPort[0];
            var port = args.Length > 1 && int.TryParse(hostAndPort[1], out var p)
                ? p
                : LctpServer.DefaultPort;

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
            int speed = 0;
            int steer = 90;
            ResponseMessage response = null;
            using (var client = new LctpClient(host, port))
            {
                Console.WriteLine("Use arrows to steer car");
                while (true)
                {
                    var key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.UpArrow:
                            response = speed < 0 ? await client.Set("motor/speed", "0") : await client.Set("motor/increasespeed", "15");
                            if (response.StatusCode == 200)
                            {
                                speed = int.Parse(response.Content);
                            }
                            else
                            {
                                Console.WriteLine(response);
                            }
                            break;
                        case ConsoleKey.DownArrow:
                            response = speed > 0 ? await client.Set("motor/speed", "0") : await client.Set("motor/decreasespeed", "15");
                            if (response.StatusCode == 200)
                            {
                                speed = int.Parse(response.Content);
                            }
                            else
                            {
                                Console.WriteLine(response);
                            }
                            break;
                        case ConsoleKey.LeftArrow:
                            response = await client.Set("steer/decrease", "15");
                            if (response.StatusCode == 200)
                            {
                                steer = int.Parse(response.Content);
                            }
                            //else
                            {
                                Console.WriteLine(response);
                            }
                            break;
                        case ConsoleKey.RightArrow:
                            response = await client.Set("steer/increase", "15");
                            if (response.StatusCode == 200)
                            {
                                steer = int.Parse(response.Content);
                            }
                            //else
                            {
                                Console.WriteLine(response);
                            }
                            break;
                        case ConsoleKey.Spacebar:
                            response = await client.Set("motor/speed", "0");
                            if (response.StatusCode == 200)
                            {
                                speed = int.Parse(response.Content);
                            }
                            else
                            {
                                Console.WriteLine(response);
                            }
                            response = await client.Set("steer/angle", "90");
                            if (response.StatusCode == 200)
                            {
                                steer = int.Parse(response.Content);
                            }
                            else
                            {
                                Console.WriteLine(response);
                            }
                            break;
                    }
                }
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine($"{Process.GetCurrentProcess().ProcessName} <host>[:port]");
        }
    }
}
