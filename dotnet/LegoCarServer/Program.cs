using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Devices.ABElectronics.ADCPiZero;
using Devices.ABElectronics.ServoPWMPiZero;
using Devices.Adafruit.BNO055;
using Devices.ThePiHut.MotoZero;
using LCTP.Core.Server;
using LCTP.Logging;
using Lego.Server;
using Lego.Server.Simulator;
using Shared;
using Unosquare.PiGpio;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.PiGpio.NativeMethods;

namespace LegoCarServer
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Handle(args);
            if (args.Contains("-simulator"))
            {
                return await ConsoleRunner.RunAsync(RunSimulatorAsync);
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
                using var pwm = new ServoPwmBoard(Board.Peripherals, Board.Pins);
                using var motoZero = new MotoZeroBoard(Board.Pins);
                var adcBoard = new ADCPiZeroBoard(Board.Peripherals);
                var imu = new BNO055Sensor(Board.Peripherals, OperationMode.NDOF);
                imu.UnitSelection.EulerAngleUnit = EulerAngleUnit.Radians;
                var car = new LegoCar(pwm, motoZero, adcBoard, imu);
            
                var controller = new LegoCarController(car);
                using var server = new LctpServer(5080, controller);
                await server.RunAsync(cancellationToken);
            }
            finally
            {
                Setup.GpioTerminate();
            }
            
        }

        private static async Task RunSimulatorAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("LegoCar Server Simulator v1.0");
            var car = new LegoCarSimulator(2);
            var controller = new LegoCarController(car);
            using (var server = new LctpServer(5080, controller))
            {
                await server.RunAsync(cancellationToken); 
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
