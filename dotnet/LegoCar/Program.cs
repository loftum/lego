using System;
using System.Collections.Generic;
using Unosquare.RaspberryIO;

namespace LegoCar
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                Console.WriteLine($"Hello {Pi.Info}");
                var picon = new PiconZeroBoard();
                //picon.Reset();
                Console.WriteLine(picon.GetRevision());
                var running = true;
                Console.CancelKeyPress += (s, a) => running = false;

                var car = new Car(0, 1, picon, new HcSr04());
                
                while (running)
                {
                    var key = Console.ReadKey(true);
                    switch (key.Key)
                    {
                        case ConsoleKey.UpArrow:
                            car.SpeedUp();
                            break;
                        case ConsoleKey.DownArrow:
                            car.SpeedDown();
                            break;
                        case ConsoleKey.LeftArrow:
                            car.SteerLeft();
                            break;
                        case ConsoleKey.RightArrow:
                            car.SteerRight();
                            break;
                        default:
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
        }
    }
}
