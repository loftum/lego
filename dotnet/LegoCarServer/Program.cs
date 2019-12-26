using System;
using System.Threading;
using System.Threading.Tasks;
using Devices.ThePiHut.ADCPiZero;
using Devices.ThePiHut.MotoZero;
using Devices.ThePiHut.ServoPWMPiZero;
using LCTP.Core.Server;
using Lego.Core;
using Shared;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

namespace LegoCarServer
{
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleRunner.Run(Run);
        }

        private static async Task Run(CancellationToken cancellationToken)
        {
            Console.WriteLine("LegoCar Server 2 v1.0");

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
