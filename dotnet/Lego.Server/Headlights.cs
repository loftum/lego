using System.Collections.Generic;
using System.Linq;
using Devices.ThePiHut.ServoPWMPiZero;
using Lego.Core;

namespace Lego.Server
{
    public class Headlights : ILight
    {
        private readonly Led[] _leds;
        private bool _on;

        public Headlights(IEnumerable<Led> leds)
        {
            _leds = leds.ToArray();
        }

        public void Toggle()
        {
            On = !On;
        }

        public bool On
        {
            get => _on;
            set
            {
                var brightness = value ? 1.0 : 0.0;
                foreach (var led in _leds)
                {
                    led.Brightness = brightness;
                }
                _on = value;
            }
        }
    }
}