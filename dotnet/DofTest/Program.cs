using System;
using System.Threading;
using System.Threading.Tasks;
using Devices.Adafruit.LSM9DS1;
using Terminal.Interactive;
using Terminal.Interactive.PoorMans;
using Unosquare.PiGpio;

namespace DofTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello!");
            using (var source = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    Console.WriteLine("Bye!");
                    Environment.Exit(0);
                };
                try
                {
                    Run(source.Token).Wait(source.Token);
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e);
                    Console.ResetColor();
                }
            }
        }

        private static async Task Run( CancellationToken token)
        {
            using (var dof = new Dof(Board.Peripherals))
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        var history = new InputHistory(new string[0], 50);
                        var controller = new PoorMansAppController(dof, token);
                        var reader = new CommandReader(controller, history);
                        var input = await reader.ReadCommand();
                        if (string.IsNullOrEmpty(input))
                        {
                            continue;
                        }
                        if (input == "quit" || input == "exit")
                        {
                            Console.WriteLine("Bye!");
                            break;
                        }
                        history.Add(input);
                        var result = await controller.ExecuteAsync(input, token);
                        Console.WriteLine(result.Pretty());
                    }
                    catch (Exception e)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(e);
                        Console.ResetColor();
                    }
                }
            }
        }
    }
}
