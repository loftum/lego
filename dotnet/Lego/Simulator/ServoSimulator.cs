using Devices.ThePiHut.ServoPWMPiZero;

namespace Lego.Simulator
{
    public class ServoSimulator : IServo
    {
        public int Value { get; set; }
    }
}