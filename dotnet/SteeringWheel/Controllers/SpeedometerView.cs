using CoreGraphics;
using UIKit;

namespace SteeringWheel.Controllers
{
    public class SpeedometerView : UIView
    {
        private readonly UIColor _outerBezelColor = new UIColor(0, 0.5f, 1, 1);
        private readonly float _outerBezelWidth = 10;

        private readonly UIColor _innerBezelColor = UIColor.White;
        private readonly float _innerBezelWidth = 5;

        private readonly UIColor _insideColor = UIColor.White;
        
        public override void Draw(CGRect rect)
        {
            var ctx = UIGraphics.GetCurrentContext();
            if (ctx == null)
            {
                return;
            }
            
            DrawBackground(rect, ctx);
        }

        private void DrawBackground(CGRect rect, CGContext ctx)
        {
            _outerBezelColor.SetColor();
            ctx.FillEllipseInRect(rect);

            // move in a little on each edge, then draw the inner bezel
            var innerBezelRect = rect.Inset(_outerBezelWidth, _outerBezelWidth);
            _innerBezelColor.SetColor();
            ctx.FillEllipseInRect(innerBezelRect);

            // finally, move in some more and draw the inside of our gauge
            var insideRect = innerBezelRect.Inset(_innerBezelWidth, _innerBezelWidth);
            _insideColor.SetColor();
            ctx.FillEllipseInRect(insideRect);
        }
    }
}