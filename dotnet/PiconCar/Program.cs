using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Devices._4tronix;
using LCTP.Core.Server;
using Lego.Server;
using Maths.Logging;
using Shared;
using Unosquare.PiGpio;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.PiGpio.NativeMethods;
using Unosquare.RaspberryIO;

namespace PiconCar
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            
            Handle(args);
            if (args.Contains("-bootstrap"))
            {
                Console.WriteLine("Bootstrapping");
                Pi.Init<BootstrapPiGpio>();
                return 0;
            }
            
            return await ConsoleRunner.RunAsync(RunAsync);
        }
        
        private static async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("LegoCar Server v1.0");
                
                Setup.GpioCfgSetInternals(ConfigFlags.NoSignalHandler); // NO SIGINT or SIGRTMIN (runtime)

                var result = Setup.GpioInitialise();
                if (result == ResultCode.InitFailed)
                {
                    throw new Exception($"Could not initialize: {result}");
                }
                using var piconZero = new PiconZeroBoard(Board.Peripherals);
                
                var car = new OrangeCar(piconZero);
            
                var controller = new LegoCarController(car);
                using var server = new LctpServer(5080, controller);
                await server.RunAsync(cancellationToken);
            }
            finally
            {
                Setup.GpioTerminate();
            }
            
        }
        
        private static void Handle(string[] args)
        {
            var arg = args.FirstOrDefault(a => a.StartsWith("-loglevel=", StringComparison.OrdinalIgnoreCase));
            if (arg == null)
            {
                return;
            }

            if (!Enum.TryParse<Importance>(arg.Substring("-loglevel=".Length), true, out var value))
            {
                return;
            }
            Console.WriteLine($"Log.Level={value}");

            Log.Level = value;
        }
    }
}