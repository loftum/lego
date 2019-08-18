using System;
using UIKit;

namespace SteeringWheel.Controllers
{
    public static class UIControlExtensions
    {
        public static T WithTouchUpInside<T>(this T slider, EventHandler handler) where T : UIControl
        {
            slider.TouchUpInside += handler;
            return slider;
        }

        public static T WithTouchUpOutside<T>(this T slider, EventHandler handler) where T : UIControl
        {
            slider.TouchUpOutside += handler;
            return slider;
        }

        public static T WithTouchDown<T>(this T slider, EventHandler handler) where T : UIControl
        {
            slider.TouchDown  += handler;
            return slider;
        }
    }
}

