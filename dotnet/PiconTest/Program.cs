using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Devices._4tronix;
using Terminal.Interactive;
using Unosquare.RaspberryIO;

namespace PiconTest
{
    class Program
    {
        private static readonly PiconZeroBoard Picon = new PiconZeroBoard(Pi.I2C);

        private static readonly List<Command> Commands = new List<Command>
        {
            new Command(@"output\[(\d+)\].type\s*=\s*(\w+);?", SetOutputType),
            new Command(@"output\[(\d+)\]\s*=\s*(\d+);?", SetOutput),
            new Command(@"motor\[(\d+)\]\s*=\s*(\d+);?", SetMotor),
        };

        private static Task<object> SetMotor(Match match)
        {
            var output = int.Parse(match.Groups[1].Value);
            var value = int.Parse(match.Groups[2].Value);
            Picon.Motors[output].Speed = value;
            return Task.FromResult<object>("OK");
        }

        private static Task<object> SetOutput(Match match)
        {
            var output = int.Parse(match.Groups[1].Value);
            var value = int.Parse(match.Groups[2].Value);
            Picon.Outputs[output].Value = value;
            return Task.FromResult<object>("OK");
        }

        private static Task<object> SetOutputType(Match match)
        {
            var output = int.Parse(match.Groups[1].Value);
            var type = (OutputType) Enum.Parse(typeof(OutputType), match.Groups[2].Value, true);
            Picon.Outputs[output].Type = type;
            return Task.FromResult<object>("OK");
        }

        static int Main(string[] args)
        {
            Console.WriteLine("PiconTest");
            
            Console.WriteLine(Picon.GetRevision());
            using (var source = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, a) => source.Cancel();
                var history = InputHistory.Load();
                try
                {
                    Run(history, source.Token).Wait(source.Token);
                    return 0;
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e);
                    return -1;
                }
                finally
                {
                    history.Save();
                    Console.WriteLine("Bye!");
                    Picon.Reset();
                }
            }
        }

        private static async Task Run(InputHistory history, CancellationToken token)
        {
            var controller = new DictionaryAppController(Commands);
            while (true)
            {
                try
                {
                    var reader = new CommandReader(controller, history);
                    var text = await reader.ReadCommand();
                    var result = await controller.ExecuteAsync(text, token);
                    Console.WriteLine(result);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }
}
