using Unosquare.PiGpio.ManagedModel;

namespace Devices.PixArt
{
    public class PAA5100JEQ_OpticalTracker
    {
        private readonly SpiChannel _spiChannel;

        public PAA5100JEQ_OpticalTracker(SpiChannel spiChannel)
        {
            _spiChannel = spiChannel;

            SecretSauce();
        }

        private void SecretSauce()
        {
            
        }
    }
}