using System;
using System.Linq;
using Devices._4tronix;
using Unosquare.RaspberryIO;

namespace ServoTest
{
    class Program
    {
        static int Main(string[] args)
        {
            var port = GetNumber(args);

            Console.WriteLine($"Hello {Pi.Info}");
            var picon = new PiconZeroBoard(Pi.I2C);
            var output = picon.Outputs[port];
            output.Type = OutputType.Servo;
            output.Value = 90;
            Console.WriteLine(picon.GetRevision());
            var running = true;
            Console.CancelKeyPress += (s, a) => running = false;
            try
            {
                Console.WriteLine("Enter servo value");
                while (running)
                {
                    Console.Write("> ");
                    var value = Console.ReadLine();
                    switch (value)
                    {
                        case "quit":
                            running = false;
                            break;
                        case "servo":
                            output.Type = OutputType.Servo;
                            break;
                        case "pwm":
                            output.Type = OutputType.Pwm;
                            break;
                        case string s when int.TryParse(s, out var number):
                            Console.WriteLine($"Value={number}");
                            output.Value = number;
                            break;
                    }
                }

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
                Console.WriteLine("Bye!");
            }
        }

        private static int GetNumber(string[] args)
        {
            if (!args.Any())
            {
                return 0;
            }
            return int.TryParse(args[0], out var number) ? number : 0;
        }
    }
}
