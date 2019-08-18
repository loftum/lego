using Unosquare.RaspberryIO.Abstractions;

namespace Devices.ThePiHut.ADCPiZero
{
    public class ADCPiZeroBoard
    {
        private const int DefaultAddress = 0x68;

        public II2CDevice Adc1 { get; }
        public II2CDevice Adc2 { get; }

        public ADCPiZeroInput[] Inputs { get; }

        public ADCPiZeroBoard(II2CBus bus) : this(bus, DefaultAddress)
        {
        }

        public ADCPiZeroBoard(II2CBus bus, int address)
        {
            Adc1 = bus.AddDevice(address);
            Adc2 = bus.AddDevice(address + 1);
            var inputs = new ADCPiZeroInput[8];
            
            for (var ii = 0; ii < 8; ii++)
            {
                var device = ii < 4 ? Adc1 : Adc2;
                inputs[ii] = new ADCPiZeroInput(device, ii);
            }
            Inputs = inputs;
        }
    }
}
