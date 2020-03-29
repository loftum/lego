using System;
using Devices.Unosquare;
using Unosquare.RaspberryIO.Abstractions;

namespace Devices.ThePiHut.ADCPiZero
{
    public interface IAnalogInput
    {
        /**
         * Returns voltage between -2.048 and 2.048 V
         */
        double ReadVoltage();
    }

    public class ADCPiZeroInput : IAnalogInput
    {
        public II2CDevice Device { get; }
        public int Channel { get; }
        public int Number { get; }
        private readonly int _baseConfig;
        public Bitrate Bitrate { get; set; } = Bitrate._12;
        public Pga Pga { get; set; } = Pga._8;
        public ConversionMode ConversionMode { get; set; } = ConversionMode.Continuous;

        public ADCPiZeroInput(II2CDevice device, int input)
        {
            Device = device;
            Channel = input < 4 ? input : input - 4;
            Number = input;
            var addressMask = 0b1111_1100 | Channel;
            _baseConfig = 0x9c & addressMask;
        }

        private int GetConfig()
        {
            var config = _baseConfig;
            switch (ConversionMode)
            {
                case ConversionMode.OneShot:
                    config &= 0b1110_1111;
                    break;
                case ConversionMode.Continuous:
                    config &= 0b1111_1111;
                    break;
            }
            switch (Bitrate)
            {
                case Bitrate._12:
                    config &= 0b1111_0011;
                    break;
                case Bitrate._14:
                    config &= 0b1111_0111;
                    break;
                case Bitrate._16:
                    config &= 0b1111_1011;
                    break;
                case Bitrate._18:
                    config &= 0b1111_1111;
                    break;
            }

            switch (Pga)
            {
                case Pga._1:
                    config &= 0b1111_1100;
                    break;
                case Pga._2:
                    config &= 0b1111_1101;
                    break;
                case Pga._4:
                    config &= 0b1111_1110;
                    break;
                case Pga._8:
                    config &= 0b1111_1111;
                    break;
            }

            return config;
        }

        private double GetLsbVoltage()
        {
            return Bitrate switch
            {
                Bitrate._12 => 0.001,
                Bitrate._14 => 0.000_250,
                Bitrate._16 => 0.000_062_5,
                Bitrate._18 => 0.000_015_625,
                _ => 9999
            };
        }

        private double GetGain()
        {
            return Pga switch
            {
                Pga._1 => 1,
                Pga._2 => 2,
                Pga._4 => 4,
                Pga._8 => 8,
                _ => 1
            };
        }

        /**
         * Returns voltage between -2.048 and 2.048 V
         */
        public double ReadVoltage()
        {
            var raw = ReadRaw();
            //Console.WriteLine($"Raw: {raw}");
            var gain = GetGain();
            var lsbVoltage = GetLsbVoltage();
            var voltage = raw * lsbVoltage / gain;
            return voltage;
        }

        public long ReadRaw()
        {
            var config = GetConfig();
            byte hi = 0;
            byte med = 0;
            byte lo = 0;
            var attempts = 0;

            while (true)
            {
                byte status = 0;
                if (Bitrate == Bitrate._18)
                {
                    var readbuffer = Device.ReadBlock(config, 4);
                    hi = readbuffer[0];
                    med = readbuffer[1];
                    lo = readbuffer[2];
                    status = readbuffer[3];
                }
                else
                {
                    var readbuffer = Device.ReadBlock(config, 3);
                    hi = readbuffer[0];
                    med = readbuffer[1];
                    status = readbuffer[2];
                }

                // check bit 7 of s to see if the conversion result is ready
                if ((status & 0b1000_0000) == 0)
                {
                    break;
                }

                if (attempts > 1000)
                {
                    Console.WriteLine("ADC timeout");
                    // timeout occurred
                    return 0;
                }
                attempts++;
            }

            return Bitrate switch
            {
                Bitrate._18 => ((hi & 3) << 16) | (med << 8) | lo,
                Bitrate._16 => (hi << 8) | med,
                Bitrate._14 => ((hi & 63) << 8) | med,
                Bitrate._12 => ((hi & 15) << 8) | med,
                _ => 0
            };
        }
    }
}
