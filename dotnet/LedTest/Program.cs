using System;
using System.Linq;
using System.Threading.Tasks;
using Devices.ThePiHut.ServoPWMPiZero;
using Shared;
using Unosquare.RaspberryIO;

namespace LedTest
{
    class Program
    {
        static int Main(string[] args)
        {
            return ConsoleRunner.Run(c => Run(args));
            
        }

        private static Task Run(string[] args)
        {
            Console.WriteLine("LED test");
            var number = GetPwmNumber(args);
            using (var board = new ServoPwmBoard(Pi.I2C, Pi.Gpio))
            {
                Console.WriteLine($"LED: {number}");
                var led = board.Outputs[number - 1].AsLed();
                led.Brightness = 0;
                try
                {
                    Console.WriteLine("Enter value [0, 1.0]");
                    while (true)
                    {
                        Console.Write("> ");
                        var text = Console.ReadLine();
                        if (text == "quit")
                        {
                            break;
                        }

                        if (double.TryParse(text, out var brightness))
                        {
                            led.Brightness = brightness;
                        }
                    }
                }
                finally
                {
                    led.Brightness = 0;
                }
                
                return Task.CompletedTask;
            }
        }

        private static int GetPwmNumber(string[] args)
        {
            if (!args.Any())
            {
                return 16;
            }
            return int.TryParse(args[0], out var number) && number > 0 && number < 17 ? number : 16;
        }
    }
}
