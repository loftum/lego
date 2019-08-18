using Devices.Unosquare;
using Unosquare.RaspberryIO.Abstractions;

namespace Devices.ThePiHut.ADCPiZero
{
    public class ADCPiZeroBoard
    {
        private const int DefaultAddress = 0x68;

        public II2CDevice Device { get; }

        public ADCPiZeroInput[] Inputs { get; }

        public ADCPiZeroBoard(II2CBus bus) : this(bus, DefaultAddress)
        {
        }

        public ADCPiZeroBoard(II2CBus bus, int address)
        {
            Device = bus.AddDevice(address);
            var inputs = new ADCPiZeroInput[8];
            
            for (var ii = 0; ii < 4; ii++)
            {
                inputs[ii] = new ADCPiZeroInput(bus.GetDeviceById(address), ii + 1);
                //inputs[ii + 4] = new ADCPiZeroInput(bus.GetDeviceById(address + 1), ii + 5);
            }
            Inputs = inputs;
        }
    }

    public class ADCPiZeroInput
    {
        public II2CDevice Device { get; }
        public int Channel { get; }
        public int Number { get; }
        public Bitrate Bitrate { get; set; } = Bitrate._12;
        public Pga Pga
        {
            get;
            set;
        }
        public ConversionMode ConversionMode { get; set; } = ConversionMode.Continuous;

        public ADCPiZeroInput(II2CDevice device, int input)
        {
            Device = device;
            Channel = input < 4 ? input : input - 4;
        }

        private int GetConfig()
        {
            byte config = 0x9c;
            switch (Channel)
            {
                case 1:
                    config &= 0b11111100;
                    break;
                case 2:
                    config &= 0b11111101;
                    break;
                case 3:
                    config &= 0b11111110;
                    break;
                case 4:
                    config &= 0b11111111;
                    break;
            }
            switch (ConversionMode)
            {
                case ConversionMode.OneShot:
                    config &= 0b11111111;
                    break;
                case ConversionMode.Continuous:
                    config &= 0b11101111;
                    break;
            }
            switch (Bitrate)
            {
                case Bitrate._12:
                    config &= 0b11110011;
                    break;
                case Bitrate._14:
                    config &= 0b11110111;
                    break;
                case Bitrate._16:
                    config &= 0b11111011;
                    break;
                case Bitrate._18:
                    config &= 0b11111111;
                    break;
            }

            switch (Pga)
            {
                case Pga._1:
                    config &= 0b11111100;
                    break;
                case Pga._2:
                    config &= 0b11111101;
                    break;
                case Pga._4:
                    config &= 0b11111110;
                    break;
                case Pga._8:
                    config &= 0b11111111;
                    break;
            }

            return config;
        }

        private double GetLsb()
        {
            switch (Bitrate)
            {
                case Bitrate._12:
                    return 0.0005;
                case Bitrate._14:
                    return 0.000125;
                case Bitrate._16:
                    return 0.00003125;
                case Bitrate._18:
                    return 0.0000078125;
                default:
                    return 9999;
            }
        }

        public double Read()
        {
            var raw = ReadRaw();
            var gain = 0.5;// 0.5 * ((int)Pga);
            var lsb = GetLsb();
            double voltage = raw * (lsb / gain) * 2.471;
            return voltage;
        }

        private long ReadRaw()
        {
            var config = GetConfig();
            byte h = 0;
            byte l = 0;
            byte m = 0;
            byte s = 0;
            long result = 0;
            int timeout = 1000; // number of reads before a timeout occurs
            int x = 0;

            do
            {
                if (Bitrate == Bitrate._18)
                {
                    var readbuffer = Device.ReadBlock(config, 4);
                    h = readbuffer[0];
                    m = readbuffer[1];
                    l = readbuffer[2];
                    s = readbuffer[3];
                }
                else
                {
                    var readbuffer = Device.ReadBlock(config, 3);
                    h = readbuffer[0];
                    m = readbuffer[1];
                    s = readbuffer[2];
                }

                // check bit 7 of s to see if the conversion result is ready
                if ((s & (1 << 7)) != 1)
                {
                    break;
                }

                if (x > timeout)
                {
                    // timeout occurred
                    return (0);
                }

                x++;
            } while (true);

            switch (Bitrate)
            {
                case Bitrate._18:
                    result = ((h & 3) << 16) | (m << 8) | l;
                    if (((result >> 17) & 1) == 1)
                    {
                        result &= ~(1 << 17);
                    }
                    break;
                case Bitrate._16:
                    result = (h << 8) | m;
                    if (((result >> 15) & 1) == 1)
                    {

                        result &= ~(1 << 15);
                    }
                    break;
                case Bitrate._14:
                    result = ((h & 63) << 8) | m;
                    if (((result >> 13) & 1) == 1)
                    {

                        result &= ~(1 << 13);
                    }
                    break;
                case Bitrate._12:
                    result = ((h & 15) << 8) | m;
                    if (((result >> 11) & 1) == 1)
                    {
                        result &= ~(1 << 11);
                    }
                    break;
            }

            return result;
        }
    }

    public enum ConversionMode
    {
        OneShot = 0,
        Continuous = 1
    }

    public enum Pga
    {
        _1 = 1,
        _2 = 2,
        _4 = 4,
        _8 = 8
    }

    public enum Bitrate
    {
        _12 = 12,
        _14 = 14,
        _16 = 16,
        _18 = 18
    }
}
