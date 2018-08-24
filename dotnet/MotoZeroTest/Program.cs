using System;
using System.Linq;
using Devices.ThePiHut.MotoZero;
using Unosquare.RaspberryIO;

namespace MotoZeroTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("MotoZero test");
            var oneBasedMotorNumber = GetMotorNumber(args);
            Console.WriteLine($"Motor: {oneBasedMotorNumber}");
            
            try
            {
                using (var gpio = Pi.Gpio)
                {
                    using (var motoZero = new MotoZeroBoard(gpio))
                    {
                        Console.WriteLine(motoZero.Motors.Length);
                        var motor = motoZero.Motors[oneBasedMotorNumber -1];
                        motor.Enabled = true;
                        motor.Speed = 0;
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
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                Console.WriteLine("Bye!");
            }
        }

        private static int GetMotorNumber(string[] args)
        {
            if (!args.Any())
            {
                return 0;
            }
            return int.TryParse(args[0], out var number) && number >= 1 && number <= 4 ? number : 0;
        }
    }
}
