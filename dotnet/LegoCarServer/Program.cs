using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Devices.Adafruit.BNO055;
using Devices.ThePiHut.ADCPiZero;
using Devices.ThePiHut.MotoZero;
using Devices.ThePiHut.ServoPWMPiZero;
using LCTP.Core.Server;
using Lego.Core;
using Lego.Simulator;
using Shared;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

namespace LegoCarServer
{
    class Program
    {
        public static async Task<int> Main(string[] args)
        {
            if (args.Contains("-simulator"))
            {
                return await ConsoleRunner.RunAsync(RunSimulatorAsync);    
            }

            return await ConsoleRunner.RunAsync(RunAsync);
        }

        private static async Task RunSimulatorAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("LegoCar Server Simulator v1.0");
            var car = new LegoCarSimulator(2);
            var controller = new LegoCarController(car);
            using (var server = new LctpServer(5080, controller))
            {
                await server.Start(cancellationToken);
            }
        }

        private static async Task RunAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("LegoCar Server v1.0");

            Pi.Init<BootstrapWiringPi>();
            using (var pwm = new ServoPwmBoard(Pi.I2C, Pi.Gpio))
            {
                using (var motoZero = new MotoZeroBoard(Pi.Gpio))
                {
                    var adcBoard = new ADCPiZeroBoard(Pi.I2C);
                    var imu = new BNO055Sensor(Pi.I2C, OperationMode.NDOF);
                    var car = new LegoCar(pwm, motoZero, adcBoard, imu);
                    var controller = new LegoCarController(car);
                    using (var server = new LctpServer(5080, controller))
                    {
                        await server.Start(cancellationToken);
                    }
                }
            }
        }
    }
}
