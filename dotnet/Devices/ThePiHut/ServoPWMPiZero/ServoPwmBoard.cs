using System;
using System.Linq;
using System.Threading;
using Unosquare.RaspberryIO.Gpio;

namespace Devices.ThePiHut.PWMServo
{
    public class Servo
    {
        private readonly Pwm _pwm;
        private int _value;

        public int MinValue { get; set; } = 20;
        public int MaxValue { get; set; } = 170;

        public int Value
        {
            get => _value;
            set
            {

                _value = value;
            }
        }

        public Servo(Pwm pwm)
        {
            _pwm = pwm;
        }


    }

    public class ServoPwmBoard
    {
        public const int LED0_ON_L = 0x06;
        public const int LED0_ON_H = 0x07;
        public const int LED0_OFF_L = 0x08;
        public const int LED0_OFF_H = 0x09;

        public const int ALL_LED_ON_L = 0xFA;

        public const int DefaultAddress = 0x40;

        public I2CDevice Device { get; }
        private readonly GpioController _gpio;
        private bool _outputEnable;

        public Pwm[] Outputs { get; }
        public Pwm AllOutputs { get; }

        public bool OutputEnable
        {
            get => _outputEnable;
            set
            {
                _gpio.Pin07.Write(!value); // active LOW
                _outputEnable = value;
            }
        }

        public void SetFrequency(int frequency, int calibration = 0)
        {
            if (frequency < 40 || frequency > 1000)
            {
                throw new ArgumentException("Frequency must be between 40 and 1000");
            }

            // 25 MHz
            var prescale = Math.Floor((double)25_000_000 / (4096 * frequency) - 1) + calibration;
            
            var oldMode = Device.ReadAddressByte((int) Registers.MODE1);
            var newMode = (oldMode & 0x7F) | 0x10; // sleep
            Device.WriteAddressByte((int)Registers.MODE1, (byte)newMode);
            Device.WriteAddressByte((int)Registers.PRE_SCALE, (byte)prescale);
            Device.WriteAddressByte((int)Registers.MODE1, oldMode);
            Thread.Sleep(TimeSpan.FromMilliseconds(5));
            Restart();
        }

        private void Restart()
        {
            var oldMode = Device.ReadAddressByte((int)Registers.MODE1);
            Device.WriteAddressByte((int)Registers.MODE1, (byte)(oldMode | 0x80));
        }

        public ServoPwmBoard(I2CBus bus, GpioController gpio) : this(bus, gpio, DefaultAddress)
        {
        }

        public ServoPwmBoard(I2CBus bus, GpioController gpio, int address)
        {
            _gpio = gpio;
            gpio.Pin07.PinMode = GpioPinDriveMode.Output;
            Device = bus.AddDevice(address);
            Outputs = Enumerable.Range(0, 16).Select(i => new Pwm(Device, i)).ToArray();
            AllOutputs = new Pwm(Device, ALL_LED_ON_L);
        }
    }
}