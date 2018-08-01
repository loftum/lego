using System;
using Devices._4tronix;
using Unosquare.RaspberryIO;

namespace ServoTest
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine($"Hello {Pi.Info}");
            var picon = new PiconZeroBoard(Pi.I2C);
            Console.WriteLine(picon.GetRevision());
            var running = true;
            Console.CancelKeyPress += (s, a) => running = false;
            using (var servo = new Servo(picon, 0))
            {
                try
                {
                    Console.WriteLine("Enter servo value");
                    while (running)
                    {
                        Console.Write("> ");
                        var value = Console.ReadLine();
                        if (value == "quit")
                        {
                            break;
                        }
                        if (int.TryParse(value, out var number))
                        {
                            servo.Angle = number;
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
                    servo.Reset();
                }
            }
        }
    }
}
