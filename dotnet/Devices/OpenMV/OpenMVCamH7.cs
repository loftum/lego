using Unosquare.PiGpio.ManagedModel;

namespace Devices.OpenMV
{
    public class OpenMVCamH7
    {
        private readonly UartPort _uart;
        
        public OpenMVCamH7(UartPort uart)
        {
            _uart = uart;
        }
    }
}