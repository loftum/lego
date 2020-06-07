using System.Collections.Generic;
using System.Linq;
using Lego.Core;

namespace Lego.Server
{
    public class Headlights : ILight
    {
        private readonly List<Led> _leds;
        private bool _on;

        public bool On
        {
            get => _on;
            set
            {
                var brightness = value ? Brightness : 0.0;
                _leds.ForEach(l => l.Brightness = brightness);
                _on = value;
            }
        }

        /// <summary>
        /// 0.0 - 1.0
        /// </summary>
        public double Brightness { get; set; } = 0.5;
        
        public Headlights(IEnumerable<Led> leds)
        {
            _leds = leds.ToList();
        }

        public void Toggle()
        {
            On = !On;
        }
    }
}