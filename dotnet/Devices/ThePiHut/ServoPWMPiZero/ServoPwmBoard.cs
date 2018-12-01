using Unosquare.RaspberryIO.Gpio;

namespace Devices.ThePiHut.PWMServo
{
    public class ServoPwmBoard
    {
        public const int DefaultAddress = 0x40;

        public I2CDevice Device { get; }

        public ServoPwmBoard(I2CBus bus) : this(bus, DefaultAddress)
        {
        }

        public ServoPwmBoard(I2CBus bus, int address)
        {
            Device = bus.AddDevice(address);
        }
    }
}