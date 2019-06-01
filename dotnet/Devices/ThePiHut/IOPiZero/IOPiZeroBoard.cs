using Unosquare.RaspberryIO.Abstractions;

namespace Devices.ThePiHut.IOPiZero
{
    public class IOPiZeroBoard
    {
        private const int DefaultAddress = 0x20;

        public II2CDevice Device { get; }

        public IOPiZeroBoard(II2CBus bus) : this(bus, DefaultAddress)
        {
        }

        public IOPiZeroBoard(II2CBus bus, int address)
        {
            Device = bus.AddDevice(address);
        }
    }
}