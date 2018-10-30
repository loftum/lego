using Unosquare.RaspberryIO.Gpio;

namespace Devices.ThePiHut.PWMServo
{
    public class PwmServoBoard
    {
        public const int I2cAddress = 0x40;

        public I2CDevice Device { get; }

        public PwmServoBoard(I2CBus bus)
        {
            Device = bus.AddDevice(I2cAddress);
        }
    }
}