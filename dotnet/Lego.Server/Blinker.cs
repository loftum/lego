using System.Collections.Generic;
using System.Linq;
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
                    _leds.ForEach(l => l.Brightness = 0);
                }
            }
        }

        private bool _isLit;
        private readonly List<Led> _leds;
        private bool _on;

        public Blinker(IEnumerable<Led> leds)
        {
            _leds = leds.ToList();
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
                    _leds.ForEach(l => l.Brightness = 0);
                    _isLit = false;
                    break;
                case false:
                    _leds.ForEach(l => l.Brightness = 1.0);
                    _isLit = true;
                    break;
            }
        }
    }
}