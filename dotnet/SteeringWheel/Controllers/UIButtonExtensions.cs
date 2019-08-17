using UIKit;

namespace SteeringWheel.Controllers
{
    public static class UIButtonExtensions
    {
        public static T WithTitle<T>(this T button, string title, UIControlState state = UIControlState.Normal) where T : UIButton
        {
            button.SetTitle(title, state);
            return button;
        }

        public static T WithTitleColor<T>(this T button, UIColor color, UIControlState state = UIControlState.Normal) where T : UIButton
        {
            button.SetTitleColor(color, state);
            return button;
        }
    }
}

