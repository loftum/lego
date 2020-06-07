using System;
using System.Linq;
using System.Threading.Tasks;
using Devices._4tronix;
using Shared;
using Unosquare.PiGpio;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.PiGpio.NativeMethods;

namespace MotorTest
{
    class Program
    {
        static int Main(string[] args)
        {
            return ConsoleRunner.Run(c => Run(args));
        }

        private static Task Run(string[] args)
        {
            try
            {
                Setup.GpioCfgSetInternals(ConfigFlags.NoSignalHandler);
                Setup.GpioInitialise();
                Console.WriteLine("Motor test");
                var number = GetMotorNumber(args);
                var picon = new PiconZeroBoard(Board.Peripherals);
                var motor = picon.Motors[number];
                Console.WriteLine($"Motor: {number}");
                motor.Speed = 0;

                Console.WriteLine("Enter motor speed");
                while (true)
                {
                    Console.Write("> ");
                    var text = Console.ReadLine();
                    if (text == "quit")
                    {
                        break;
                    }

                    if (int.TryParse(text, out var speed))
                    {
                        motor.Speed = speed;
                    }
                }
                return Task.CompletedTask;
            }
            finally
            {
                Setup.GpioTerminate();
            }
            
        }

        private static int GetMotorNumber(string[] args)
        {
            if (!args.Any())
            {
                return 0;
            }
            return int.TryParse(args[0], out var number) && number > 0 && number < 2 ? number : 0;
        }
    }
}
