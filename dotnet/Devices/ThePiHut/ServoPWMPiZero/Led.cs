namespace Devices.ThePiHut.ServoPWMPiZero
{
    public class Led
    {
        private readonly ServoPwmBoard _board;
        private readonly Pwm _pwm;
        private double _brightness;

        public Led(Pwm pwm, ServoPwmBoard board)
        {
            _pwm = pwm;
            _board = board;
            Brightness = 0;
        }

        public double Brightness
        {
            get => _brightness;
            set
            {
                if (value < 0.0 || value > 1.0)
                {
                    return;
                }

                var time = (int) (value * 4095.0);
                _pwm.OnTime = 0;
                _pwm.OffTime = time;
                _brightness = value;
            }
        }
    }
}