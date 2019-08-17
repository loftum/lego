using System;
using System.Linq;
using System.Threading;
using Unosquare.RaspberryIO.Abstractions;

namespace Devices._4tronix
{
    public class PiconZeroBoard: IDisposable
    {
        public const int I2CAddress = 0x22;
        public II2CDevice Device { get; }

        private const byte MOTORA = 0;
        public const byte OUTCFG0 = 2;
        public const byte OUTPUT0 = 8;
        public const byte INCFG0 = 14;
        private const int SETBRIGHT = 18;
        private const int UPDATENOW = 19;
        private const int RESET = 20;
        private readonly int _retries = 10;

        public InputPort[] Inputs { get; }
        public OutputPort[] Outputs { get; }
        public MotorPort[] Motors { get; }

        public PiconZeroBoard(II2CBus bus)
        {
            Device = bus.AddDevice(I2CAddress);
            Inputs = Enumerable.Range(0, 4).Select(i => new InputPort(Device, i)).ToArray();
            Outputs = Enumerable.Range(0, 6).Select(i => new OutputPort(Device, i)).ToArray();
            Motors = Enumerable.Range(0, 2).Select(i => new MotorPort(Device, i)).ToArray();
        }

        public string GetRevision()
        {
            var value = Device.ReadAddressWord(0);
            return $"{value / 256} {value % 256}";
        }

        private void Try(Action action)
        {
            for (var ii = 0; ii < _retries; ii++)
            {
                try
                {
                    action();
                }
                catch
                {
                    Thread.Sleep(1);
                }
            }
        }

        public void SetOutputConfig(int pin, OutputType type)
        {
            if (pin < 0 || pin > 5)
            {
                throw new ArgumentException("Output pin must be 0-5", nameof(pin));
            }
            Try(() => Device.WriteAddressByte(OUTCFG0 + pin, (byte) type));
        }

        public void Dispose()
        {
            
        }
    }
}