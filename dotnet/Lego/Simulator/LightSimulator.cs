using Lego.Core;

namespace Lego.Simulator
{
    public class LightSimulator : ILight
    {
        public bool On { get; set; }
        
        public void Toggle()
        {
            On = !On;
        }
    }
}