using Unosquare;
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
        static void Main(string[] args)
        {
            try
            {
                Pi.Init<BootstrapWiringPi>();
                var input = args.Length > 0 && int.TryParse(args[0], out var v) ? v : 0;
                using (var source = new CancellationTokenSource())
                {
                    Console.CancelKeyPress += (s, e) => source.Cancel();
                    Console.WriteLine("Distance sensor test");
                    Task.Run(() => Run(input, source.Token), source.Token).Wait();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return;
            }
        }

        private static async Task Run(int inputNumber, CancellationToken token)
        {
            var board = new ADCPiZeroBoard(Pi.I2C);
            var input = board.Inputs[inputNumber];
            input.Bitrate = Bitrate._16;
            input.ConversionMode = ConversionMode.Continuous;
            

            double lastRead = 0;
            while (!token.IsCancellationRequested)
            {
                var read = input.Read();
                if (Math.Abs(read - lastRead) > .1)
                {
                    Console.WriteLine($"Read: {read}");
                }
                await Task.Delay(100);
            }
        }
    }
}
