using System;
using System.Linq;
using System.Threading;
using Unosquare.PiGpio.ManagedModel;
using Unosquare.PiGpio.NativeEnums;

namespace Devices.ThePiHut.ServoPWMPiZero
{
    public class ServoPwmBoard : IDisposable
    {
        public const byte DefaultAddress = 0x40;

        public I2cDevice Device { get; }
        private readonly GpioPinCollection _gpio;
        private bool _outputEnable;

        /// <summary>
        /// in herz
        /// </summary>
        public int Frequency { get; private set; }
        public Pwm[] Outputs { get; }
        public Pwm OutputAll { get; }

        public bool OutputEnable
        {
            get => _outputEnable;
            set
            {
                _gpio[7].Write(value ? 0 : 1); // active LOW
                _outputEnable = value;
            }
        }

        public void SetFrequency(int hertz, int calibration = 0)
        {
            if (hertz < 40 || hertz > 1000)
            {
                throw new ArgumentException("Frequency must be between 40 and 1000");
            }

            // 25 MHz
            var prescale = Math.Round(25_000_000.0 / (4096 * hertz) - 1) + calibration;
            
            var oldMode = Device.ReadByte((int) Registers.MODE1);
            var newMode = (oldMode & 0x7F) | 0x10; // sleep
            Device.Write((int)Registers.MODE1, (byte)newMode);
            Device.Write((int)Registers.PRE_SCALE, (byte)prescale);
            Device.Write((byte)Registers.MODE1, oldMode);
            Frequency = hertz;
            Thread.Sleep(TimeSpan.FromMilliseconds(5));
            Restart();
        }

        private void Restart()
        {
            var oldMode = Device.ReadByte((int)Registers.MODE1);
            Device.Write((int)Registers.MODE1, (byte)(oldMode | 0x80));
        }

        public ServoPwmBoard(BoardPeripheralsService bus, GpioPinCollection gpio) : this(bus, gpio, DefaultAddress)
        {
        }

        public ServoPwmBoard(BoardPeripheralsService bus, GpioPinCollection gpio, byte address)
        {
            _gpio = gpio;
            
            gpio[7].Direction = PinDirection.Output;
            Device = bus.OpenI2cDevice(address);
            ConfigureMode1(0x00);
            Device.Write((int)Registers.MODE2, 0x0c);
            SetFrequency(50);
            Outputs = Enumerable.Range(0, 16).Select(i => new Pwm(Device, this, i)).ToArray();
            OutputAll = new Pwm(Device, this, (int) Registers.ALL_LED_ON_L);
        }

        private void ConfigureMode1(byte config)
        {
            Device.Write((int)Registers.MODE1, config);
        }

        private bool _isDisposed;
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
        }
    }

    [Flags]
    public enum Mode1
    {
        None = 0,
        Restart = 0b1000_0000,
        ExternalClockEnabled = 0b0100_0000,
        AutoIncrementEnabled = 0b0010_0000,
        Sleep = 0b0001_0000,
        SubAddress1Enabled = 0b0000_1000,
        SubAddress2Enabled = 0b0000_0100,
        SubAddress3Enabled = 0b0000_0010,
        AllCallEnabled = 0b0000_0001
    }
}