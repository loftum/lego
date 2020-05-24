using System;
using Unosquare.RaspberryIO;
using Devices.ThePiHut.ADCPiZero;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.WiringPi;

namespace DistanceSensorTest
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                Pi.Init<BootstrapWiringPi>();
                var input = args.Length > 0 && int.TryParse(args[0], out var v) ? v : 0;
                using (var source = new CancellationTokenSource())
                {
                    Console.CancelKeyPress += (s, e) => source.Cancel();
                    Console.WriteLine("Distance sensor test");
                    await RunAsync(input, source.Token);
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("Bye");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return -1;
            }

            return 0;
        }

        private static async Task RunAsync(int inputNumber, CancellationToken token)
        {
            var board = new ADCPiZeroBoard(Pi.I2C);
            var input = board.Inputs[inputNumber];
            input.Pga = Pga._1;
            input.Bitrate = Bitrate._14;
            input.ConversionMode = ConversionMode.Continuous;

            double lastRead = 0;
            while (!token.IsCancellationRequested)
            {
                var read = input.ReadVoltage();
                if (Math.Abs(read - lastRead) > .1)
                {
                    Console.WriteLine($"Read: {read}");
                }

                lastRead = read;
                await Task.Delay(100, token);
            }
        }
    }
}
