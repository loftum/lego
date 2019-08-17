using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using UIKit;

namespace SteeringWheel.Controllers
{
    public static class UIViewExtensions
    {
        public static T WithParent<T>(this T view, UIView parent) where T : UIView
        {
            parent.AddSubview(view);
            return view;
        }

        public static T WithTintColor<T>(this T view, UIColor color) where T : UIView
        {
            view.TintColor = color;
            return view;
        }

        public static T Rotate<T>(this T view, double angle) where T : UIView
        {
            CGAffineTransform trans = CGAffineTransform.MakeRotation((nfloat)angle);
            view.Transform = trans;
            return view;
        }

        public static T WithConstraints<T>(this T view, Func<T, IEnumerable<NSLayoutConstraint>> constraints) where T : UIView
        {
            view.TranslatesAutoresizingMaskIntoConstraints = false;
            NSLayoutConstraint.ActivateConstraints(constraints(view).ToArray());
            return view;
        }

        public static T WithBackgroundColor<T>(this T view, UIColor color) where T : UIView
        {
            view.BackgroundColor = color;
            return view;
        }

        public static T With<T>(this T view, Action<T> action) where T : UIView
        {
            action(view);
            return view;
        }
    }
}

