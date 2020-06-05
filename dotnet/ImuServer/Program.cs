using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Devices.Adafruit.BNO055;
using LCTP.Core.Server;
using Shared;
using Unosquare.PiGpio;
using Unosquare.RaspberryIO;
using Unosquare.WiringPi;

namespace ImuServer
{
    class Program
    {
        private static bool Verbose;
        
        static async Task<int> Main(string[] args)
        {
            Verbose = args.Contains("-v");
            return await ConsoleRunner.RunAsync(RunAsync);
        }
        
        private static async Task RunAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("ImuServer v1.0");

            Pi.Init<BootstrapWiringPi>();
            var sensor = new BNO055Sensor(Board.Peripherals, OperationMode.NDOF);
            sensor.UnitSelection.EulerAngleUnit = EulerAngleUnit.Radians;
            using (var controller = new ImuController(sensor, Verbose))
            {
                using (var server = new LctpTcpServer(5080, controller))
                {
                    await server.RunAsync(cancellationToken);
                }    
            }
        }
    }
}