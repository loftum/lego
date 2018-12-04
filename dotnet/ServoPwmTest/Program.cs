using System;
using System.Linq;
using System.Threading.Tasks;
using Devices.ThePiHut.ServoPWMPiZero;
using Shared;
using Unosquare.RaspberryIO;

namespace ServoPwmTest
{
    class Program
    {
        static int Main(string[] args)
        {
            return ConsoleRunner.Run(() => Run(args));
        }

        private static Task Run(string[] args)
        {
            var number = args.Any() && int.TryParse(args[0], out var n) ? n : 0;
            using (var board = new ServoPwmBoard(Pi.I2C, Pi.Gpio))
            {
                Console.WriteLine("PwmServo test");
                Console.WriteLine($"Frequency: {board.Frequency}Hz");
                var servo = board.Outputs[number].AsServo();
                Console.WriteLine($"Servo.MinPos: {servo.MinPos}");
                Console.WriteLine($"Servo.MaxPos: {servo.MaxPos}");
                Console.WriteLine($"Servo.MinValue: {servo.MinValue}");
                Console.WriteLine($"Servo.MaxValue: {servo.MaxValue}");
                servo.Value = 90;

                while (true)
                {
                    Console.WriteLine("Enter degrees");
                    Console.Write("> ");
                    var read = Console.ReadLine();
                    switch (read)
                    {
                        case null:
                            break;
                        case "quit":
                            return Task.CompletedTask;
                        default:
                            if (int.TryParse(read, out var angle))
                            {
                                servo.Value = angle;
                            }

                            break;
                    }
                }
            }
        }
    }
}
