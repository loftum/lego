using System;
using System.Threading;
using Unosquare.RaspberryIO.Gpio;

namespace Devices._4tronix
{
    public class PiconZeroBoard
    {
        public const int I2CAddress = 0x22;
        private readonly I2CDevice _device;

        private const int MOTORA = 0;
        private const int OUTCFG0 = 2;
        private const int OUTPUT0 = 8;
        private const int INCFG0 = 14;
        private const int SETBRIGHT = 18;
        private const int UPDATENOW = 19;
        private const int RESET = 20;
        private readonly int _retries = 10;

        public PiconZeroBoard(I2CBus bus)
        {
            _device = bus.AddDevice(I2CAddress);
        }

        public string GetRevision()
        {
            var value = _device.ReadAddressWord(0);
            return $"{value / 256} {value % 256}";
        }

        public void SetMotor(int motor, int speed)
        {
            if (motor < 0 || motor > 1 || Math.Abs(speed) > 127)
            {
                return;
            }
            Try(() => _device.WriteAddressByte(motor, (byte)speed));
        }

        public void SetOutput(int channel, int value)
        {
            if (channel < 0 || channel > 5)
            {
                return;
            }
            Try(() => _device.WriteAddressByte(OUTPUT0 + channel, (byte)value));
        }

        public void Reset()
        {
            Try(() => _device.WriteAddressByte(RESET, 0));
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
            Try(() => _device.WriteAddressByte(OUTCFG0 + pin, (byte) type));
        }

        public void SetInputConfig(int pin, InputType type)
        {
            if (pin < 0 || pin > 3)
            {
                throw new ArgumentException("Input pin must be 0-3", nameof(pin));
            }
            Try(() => _device.WriteAddressByte(INCFG0 + pin, (byte)type));
        }
    }

    public enum InputType
    {
        Digital = 0,
        Analog = 1
    }

    public enum OutputType
    {
        OnOff = 0,
        Pwm = 1,
        Servo = 2,
        Ws2812B = 3
    }
}