using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Devices.ABElectronics.ServoPWMPiZero;
using Lego.Server;
using Shared;
using Unosquare.PiGpio;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.PiGpio.NativeMethods;

namespace LedTest
{
    class Program
    {
        static Task<int> Main(string[] args)
        {
            return ConsoleRunner.RunAsync(RunAsync);
        }

        private static async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                Setup.GpioCfgSetInternals(ConfigFlags.NoSignalHandler);
                Setup.GpioInitialise();
                using (var board = new ServoPwmBoard(Board.Peripherals, Board.Pins))
                {
                    var leds = board.Outputs.Select(o => o.AsLed()).ToList();
                    try
                    {
                        PrintUsage();
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var line = Console.ReadLine();
                            Parse(line, leds);
                        }
                    }
                    finally
                    {
                        foreach (var led in leds)
                        {
                            led.Brightness = 0;
                        }
                    }
                }
            }
            finally
            {
                Setup.GpioTerminate();
            }
        }

        private static void Parse(string line, IList<Led> leds)
        {
            if (line.Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                PrintUsage();
                return;
            }
            var parts = line.Split('=');
            if (parts.Length != 2)
            {
                return;
            }

            if (!int.TryParse(parts[0], out var pin) || pin < 0 || pin > 15)
            {
                Console.WriteLine($"Invalid pin: {parts[0]}");
                return;
            }

            if (!double.TryParse(parts[1], out var brightness) || brightness < 0.0 || brightness > 1.0)
            {
                Console.WriteLine($"Invalid value: {parts[1]}");
                return;
            }

            leds[pin].Brightness = brightness;
        }

        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("<pin>=<value>");
            Console.WriteLine("pin:0-15, value: 0.0-1.0");
        }
    }
}