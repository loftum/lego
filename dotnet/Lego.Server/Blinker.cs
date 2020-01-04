using Devices.ThePiHut.ServoPWMPiZero;
using Lego.Core;

namespace Lego.Server
{
    public class Blinker : ILight
    {
        public bool On
        {
            get => _on;
            set
            {
                _on = value;
                if (!_on)
                {
                    _led.Brightness = 0;
                }
            }
        }

        private bool _isLit;
        private readonly Led _led;
        private bool _on;

        public Blinker(Led led)
        {
            _led = led;
        }

        public void Toggle()
        {
            if (!On)
            {
                return;
            }
            switch (_isLit)
            {
                case true:
                    _led.Brightness = 0;
                    _isLit = false;
                    break;
                case false:
                    _led.Brightness = 1.0;
                    _isLit = true;
                    break;
            }
        }
    }
}