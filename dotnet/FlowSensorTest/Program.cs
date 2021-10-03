using System;
using System.Threading;
using System.Threading.Tasks;
using Devices.PixArt;
using Shared;
using Unosquare.PiGpio;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.PiGpio.NativeMethods;

namespace FlowSensorTest
{
    class Program
    {
        static Task<int> Main(string[] args)
        {
            return ConsoleRunner.RunAsync(c => Run(args, c));
        }

        private static async Task Run(string[] args, CancellationToken cancellationToken)
        {
            try
            {
                Setup.GpioCfgSetInternals(ConfigFlags.NoSignalHandler);
                Setup.GpioInitialise();
                Console.WriteLine("FlowSensor test");

                var channel = Board.Peripherals.OpenSpiChannel(SpiChannelId.SpiChannel0, 400000);
                
                var sensor = new PAA5100JEQ_FlowSensor(channel, Board.Pins[UserGpio.Bcm07]);
                await sensor.InitAsync(cancellationToken);
                var timeout = TimeSpan.FromSeconds(5);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var (x, y) = sensor.get_motion(timeout);
                    Console.WriteLine($"x: {x}, y: {y}");
                    await Task.Delay(100, cancellationToken);
                }
            }
            finally
            {
                Setup.GpioTerminate();
            }
        }
    }
}