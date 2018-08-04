using System;
using System.Linq;
using System.Threading;
using Unosquare.RaspberryIO.Gpio;

namespace Devices._4tronix
{
    public class PiconZeroBoard
    {
        public const int I2CAddress = 0x22;
        public I2CDevice Device { get; }

        private const int MOTORA = 0;
        public const int OUTCFG0 = 2;
        public const int OUTPUT0 = 8;
        public const int INCFG0 = 14;
        private const int SETBRIGHT = 18;
        private const int UPDATENOW = 19;
        private const int RESET = 20;
        private readonly int _retries = 10;

        public InputPort[] Inputs { get; }
        public OutputPort[] Outputs { get; }
        public MotorPort[] Motors { get; }

        public PiconZeroBoard(I2CBus bus)
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

        public void SetMotor(int motor, int speed)
        {
            if (motor < 0 || motor > 1 || Math.Abs(speed) > 127)
            {
                return;
            }
            Try(() => Device.WriteAddressByte(motor, (byte)speed));
        }

        public void SetOutput(int channel, int value)
        {
            if (channel < 0 || channel > 5)
            {
                return;

            }
            Try(() => Device.WriteAddressByte(OUTPUT0 + channel, (byte)value));
        }

        public void Reset()
        {
            Try(() => Device.WriteAddressByte(RESET, 0));
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

        private T Try<T>(Func<T> get)
        {
            for (var ii = 0; ii < _retries; ii++)
            {
                try
                {
                    return get();
                }
                catch
                {
                    //
                }
            }
            return default(T);
        }

        public void SetOutputConfig(int pin, OutputType type)
        {
            if (pin < 0 || pin > 5)
            {
                throw new ArgumentException("Output pin must be 0-5", nameof(pin));
            }
            Try(() => Device.WriteAddressByte(OUTCFG0 + pin, (byte) type));
        }

        public void SetInputConfig(int pin, InputType type)
        {
            if (pin < 0 || pin > 3)
            {
                throw new ArgumentException("Input pin must be 0-3", nameof(pin));
            }
            Try(() => Device.WriteAddressByte(INCFG0 + pin, (byte)type));
        }
    }
}