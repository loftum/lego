using System;
using System.Linq;
using System.Threading;
using Unosquare.RaspberryIO.Gpio;

namespace Devices.ThePiHut.PWMServo
{
    public enum Registers
    {
        MODE1 = 0x00,
        MODE2 = 0x01,
        SUABDR1 = 0x02,
        SUBADR2 = 0x03,
        ALLCALLADR = 0x05,
        LED0_ON_L = 0x06,
        LED0_ON_H = 0x07,
        LED0_OFF_L = 0x08,
        LED0_OFF_H = 0x09,

        ALL_LED_ON_L = 0xFA,
        ALL_LED_ON_H = 0xFB,
        ALL_LED_OFF_L = 0xFC,
        ALL_LED_OFF_H = 0xFD,
        PRE_SCALE = 0xFE,
        TestMode = 0xFF
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

        public Pwm[] Outputs { get; }
        public Pwm AllOutputs { get; }

        public void SetFrequency(int frequency, int calibration = 0)
        {
            if (frequency < 40 || frequency > 1000)
            {
                throw new ArgumentException($"Frequency must be between 40 and 1000");
            }

            var scaleVal = 25_000_000.0; // 25 MHz
            scaleVal /= 4096; // 12 bit
            scaleVal /= frequency;
            scaleVal -= 1;
            var prescale = Math.Floor(scaleVal + 0.5);
            prescale = prescale + calibration;
            var oldMode = Device.ReadAddressByte((int) Registers.MODE1);
            var newMode = (oldMode & 0x7F) | 0x10;
            Device.WriteAddressByte((int)Registers.MODE1, (byte)newMode);
            Device.WriteAddressByte((int)Registers.PRE_SCALE, (byte)prescale);
            Device.WriteAddressByte((int)Registers.MODE1, oldMode);
            Thread.Sleep(TimeSpan.FromMilliseconds(5));
            Device.WriteAddressByte((int) Registers.MODE1, (byte)(oldMode | 0x80));
        }

        public ServoPwmBoard(I2CBus bus) : this(bus, DefaultAddress)
        {
        }

        public ServoPwmBoard(I2CBus bus, int address)
        {
            Device = bus.AddDevice(address);
            Outputs = Enumerable.Range(0, 16).Select(i => new Pwm(Device, i)).ToArray();
            AllOutputs = new Pwm(Device, ALL_LED_ON_L);
        }
    }
}