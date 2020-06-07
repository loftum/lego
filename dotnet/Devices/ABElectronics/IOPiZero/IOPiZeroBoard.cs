using Unosquare.PiGpio.ManagedModel;

namespace Devices.ABElectronics.IOPiZero
{
    public class IOPiZeroBoard
    {
        private const byte DefaultAddress = 0x20;

        public I2cDevice Device { get; }

        public IOPiZeroBoard(BoardPeripheralsService bus) : this(bus, DefaultAddress)
        {
        }

        public IOPiZeroBoard(BoardPeripheralsService bus, byte address)
        {
            Device = bus.OpenI2cDevice(address);
        }
    }
}