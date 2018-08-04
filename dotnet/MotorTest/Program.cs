using System;
using System.Linq;
using Devices._4tronix;
using Unosquare.RaspberryIO;

namespace MotorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Motor test");
            var number = GetMotorNumber(args);
            var picon = new PiconZeroBoard(Pi.I2C);
            var motor = picon.Motors[number];
            Console.WriteLine($"Motor: {number}");
            motor.Speed = 0;
            try
            {
                var running = true;
                Console.WriteLine("Enter motor speed");
                while (running)
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                motor.Speed = 0;
                Console.WriteLine("Bye!");
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
