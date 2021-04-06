using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.PiGpio;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.PiGpio.NativeMethods;

namespace OpenMVTest
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            using var source = new CancellationTokenSource();
            try
            {
                Console.CancelKeyPress += (s, e) => source.Cancel();
                
                Setup.GpioCfgSetInternals(ConfigFlags.NoSignalHandler); // NO SIGINT or SIGRTMIN (runtime)

                var result = Setup.GpioInitialise();
                if (result == ResultCode.InitFailed)
                {
                    throw new Exception($"Could not initialize: {result}");
                }

                await RunAsync(source.Token);

                return 0;
            }
            catch (TaskCanceledException)
            {
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }
            finally
            {
                Setup.GpioTerminate();
            }
        }

        private static async Task RunAsync(CancellationToken cancellationToken)
        {
            using var uart = Board.Peripherals.OpenUartPort("/dev/serial1", UartRate.BaudRate19200);
            while (!cancellationToken.IsCancellationRequested)
            {
                if (uart.Available > 0)
                {
                    var bytes = uart.Read(uart.Available);
                    var message = Encoding.UTF8.GetString(bytes);
                    Console.WriteLine(message);
                }

                await Task.Delay(500, cancellationToken);
            }
        }
    }
}