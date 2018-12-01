using Unosquare.RaspberryIO.Gpio;

namespace Devices.ThePiHut.IOPiZero
{
    public class IOPiZeroBoard
    {
        private const int DefaultAddress = 0x20;

        public I2CDevice Device { get; }

        public IOPiZeroBoard(I2CBus bus) : this(bus, DefaultAddress)
        {
        }

        public IOPiZeroBoard(I2CBus bus, int address)
        {
            Device = bus.AddDevice(address);
        }
    }
}