using System;
using System.Collections.Generic;
using System.Linq;
using Unosquare.RaspberryIO;
using System.Threading;
using System.Threading.Tasks;
using Devices.ABElectronics.ADCPiZero;
using Unosquare.PiGpio;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.PiGpio.NativeMethods;

namespace DistanceSensorTest
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                Setup.GpioCfgSetInternals(ConfigFlags.NoSignalHandler);
                Setup.GpioInitialise();
                var inputs = args.Select(a => int.TryParse(a, out var v) ? v : -1)
                    .Where(v => v >= 0 && v <= 7)
                    .Distinct()
                    .ToList();
                if (!inputs.Any())
                {
                    inputs = new List<int> {0};
                }

                using (var source = new CancellationTokenSource())
                {
                    Console.CancelKeyPress += (s, e) => source.Cancel();
                    Console.WriteLine("Distance sensor test");
                    await RunAsync(inputs, source.Token);
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
            finally
            {
                Setup.GpioTerminate();
            }

            return 0;
        }

        private static async Task RunAsync(IEnumerable<int> inputNumbers, CancellationToken token)
        {
            var inputs = GetInputs(inputNumbers);
            
            var lastReadings = new List<Reading>();
            while (!token.IsCancellationRequested)
            {
                var values = new List<Reading>();
                foreach (var input in inputs)
                {
                    values.Add(new Reading(input.Number, input.ReadVoltage()));
                    await Task.Delay(25, token);
                }
                if (values.Any(v => lastReadings.Any(r => r.InputNumber == v.InputNumber && Math.Abs(v.Voltage - r.Voltage) > .005)))
                {
                    Console.WriteLine(string.Join(", ", values.Select(v => $"{v.InputNumber}: {v.Voltage}")));
                }

                lastReadings = values;
                
                await Task.Delay(100, token);
            }
        }

        private static List<ADCPiZeroInput> GetInputs(IEnumerable<int> inputNumbers)
        {
            var board = new ADCPiZeroBoard(Board.Peripherals);
            var inputs = inputNumbers.Select(i => board.Inputs[i]).ToList();
            foreach (var input in inputs)
            {
                input.Pga = Pga._1;
                input.Bitrate = Bitrate._14;
                input.ConversionMode = ConversionMode.Continuous;    
            }

            return inputs;
        }
    }

    internal readonly struct Reading
    {
        internal int InputNumber { get; }
        internal double Voltage { get; }
        
        public Reading(int inputNumber, double voltage)
        {
            InputNumber = inputNumber;
            Voltage = voltage;
        }
    }
}
