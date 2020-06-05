using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Devices.Adafruit.BNO055;
using Devices.ThePiHut.ADCPiZero;
using Devices.ThePiHut.MotoZero;
using Devices.ThePiHut.ServoPWMPiZero;
using LCTP.Core.Server;
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
            try
            {
                Setup.GpioCfgSetInternals(ConfigFlags.NoSignalHandler); // NO SIGINT or SIGRTMIN (runtime)
                Setup.GpioInitialise();
                if (args.Contains("-simulator"))
                {
                    return await ConsoleRunner.RunAsync(RunSimulatorAsync);
                }
                return await ConsoleRunner.RunAsync(RunAsync);
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
            using (var server = new LctpTcpServer(5080, controller))
            {
                await server.RunAsync(cancellationToken); 
            }
        }

        private static async Task RunAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("LegoCar Server v1.0");
            
            using (var pwm = new ServoPwmBoard(Board.Peripherals, Board.Pins))
            {
                using (var motoZero = new MotoZeroBoard(Board.Pins))
                {
                    var adcBoard = new ADCPiZeroBoard(Board.Peripherals);
                    var imu = new BNO055Sensor(Board.Peripherals, OperationMode.NDOF);
                    imu.UnitSelection.EulerAngleUnit = EulerAngleUnit.Radians;
                    var car = new LegoCar(pwm, motoZero, adcBoard, imu);
                    
                    //var car = new LegoCar(pwm, motoZero, adcBoard, null);
                    var controller = new LegoCarController(car);
                    using (var server = new LctpUdpServer(5081, controller))
                    {
                        await server.RunAsync(cancellationToken);
                    }
                }
            }
        }
    }
}
