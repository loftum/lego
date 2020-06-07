using Unosquare.PiGpio.ManagedModel;

namespace Devices.ABElectronics.ADCPiZero
{
    public class ADCPiZeroBoard
    {
        private const byte DefaultAddress = 0x68;

        public I2cDevice Adc1 { get; }
        public I2cDevice Adc2 { get; }

        public ADCPiZeroInput[] Inputs { get; }

        public ADCPiZeroBoard(BoardPeripheralsService bus) : this(bus, DefaultAddress)
        {
        }

        public ADCPiZeroBoard(BoardPeripheralsService bus, byte address)
        {
            Adc1 = bus.OpenI2cDevice(address);
            Adc2 = bus.OpenI2cDevice((byte)(address + 1));
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
