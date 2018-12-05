using System;
using System.Linq;
using System.Threading.Tasks;
using Devices._4tronix;
using Shared;
using Unosquare.RaspberryIO;

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
            Console.WriteLine("Motor test");
            var number = GetMotorNumber(args);
            var picon = new PiconZeroBoard(Pi.I2C);
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
