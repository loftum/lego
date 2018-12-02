using Unosquare.RaspberryIO.Gpio;

namespace Devices.ThePiHut.PWMServo
{
    public class Pwm
    {
        private readonly I2CDevice _device;
        private readonly int _address;
        private int _onTime;
        private int _offTime;

        public int OnTime
        {
            get => _onTime;
            set
            {
                _device.WriteAddressByte(_address + 2, (byte)(value & 0xFF));
                _device.WriteAddressByte(_address + 3, (byte)(value >> 8));
                _onTime = value;
            }
        }

        public int OffTime
        {
            get => _offTime;
            set
            {
                _device.WriteAddressByte(_address, (byte)(value & 0xFF));
                _device.WriteAddressByte(_address + 1, (byte)(value >> 8));
                _offTime = value;
            }
        }

        public Pwm(I2CDevice device, int outputNumber)
        {
            _device = device;
            _address = ServoPwmBoard.LED0_OFF_L + 4 * outputNumber;
        }
    }
}