using System.Collections.Generic;
using System.Linq;
using Devices.ThePiHut.ServoPWMPiZero;

namespace LegoCarServer2
{
    public class Headlights
    {
        private readonly Led[] _leds;
        private bool _on;

        public Headlights(IEnumerable<Led> leds)
        {
            _leds = leds.ToArray();
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