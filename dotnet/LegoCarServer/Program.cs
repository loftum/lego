using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        static void Main(string[] args)
        {
            if (args.Contains("-simulator"))
            {
                ConsoleRunner.Run(RunSimulator);    
            }
            else
            {
                ConsoleRunner.Run(Run);    
            }
        }

        private static async Task RunSimulator(CancellationToken cancellationToken)
        {
            Console.WriteLine("LegoCar Server Simulator v1.0");
            var car = new LegoCarSimulator(2);
            var controller = new LegoCarController(car);
            using (var server = new LctpServer(5080, controller))
            {
                await server.Start(cancellationToken);
            }
        }

        private static async Task Run(CancellationToken cancellationToken)
        {
            Console.WriteLine("LegoCar Server Simulator v1.0");

            Pi.Init<BootstrapWiringPi>();
            using (var pwm = new ServoPwmBoard(Pi.I2C, Pi.Gpio))
            {
                using (var motoZero = new MotoZeroBoard(Pi.Gpio))
                {
                    var adcBoard = new ADCPiZeroBoard(Pi.I2C);
                    var car = new LegoCar(pwm, motoZero, adcBoard);
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
