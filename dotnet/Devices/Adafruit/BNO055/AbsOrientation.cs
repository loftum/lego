using System;
using Unosquare.RaspberryIO.Gpio;

namespace Devices.Adafruit.BNO055
{
    public enum OperationMode
    {
        CONFIG = 0X00,
        ACCONLY = 0X01,
        MAGONLY = 0X02,
        GYRONLY = 0X03,
        ACCMAG = 0X04,
        ACCGYRO = 0X05,
        MAGGYRO = 0X06,
        AMG = 0X07,
        IMUPLUS = 0X08,
        COMPASS = 0X09,
        M4G = 0X0A,
        NDOF_FMC_OFF = 0X0B,
        NDOF = 0X0C
    }

    public class AbsOrientation
    {
        public const int Id = 0xA0;
        public const int DefaultI2CAddress = 0x29;
        public const int AlternativeI2CAddress = 0x29;

        private readonly I2CDevice _device;

        public AbsOrientation(I2CBus bus)
        {
            _device = bus.AddDevice(DefaultI2CAddress);
            var id = _device.ReadAddressByte((int) Registers.BNO055_CHIP_ID_ADDR);
            if (id != Id)
            {
                throw new ApplicationException($"Unexpected id. Expected {Id}, but got {id}");
            }

            SetOperationMode(OperationMode.CONFIG);
        }

        private void SetOperationMode(OperationMode mode)
        {
            _device.WriteAddressByte((int)Registers.BNO055_OPR_MODE_ADDR, (byte)mode);
        }
    }
}