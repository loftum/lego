using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;


namespace Visualizer.ViewControllers
{
    public static class NSViewExtensions
    {
        public static T WithParent<T>(this T view, NSView parent) where T : NSView
        {
            parent.AddSubview(view);
            return view;
        }
        
        public static TChild WithParent<TChild, TParent>(this TChild child, TParent parent, Func<TChild, TParent, IEnumerable<NSLayoutConstraint>> constraints)
            where TChild : NSView where TParent : NSView
        {
            parent.AddSubview(child);
            child.TranslatesAutoresizingMaskIntoConstraints = false;
            NSLayoutConstraint.ActivateConstraints(constraints(child, parent).ToArray());
            return child;
        }
        
        public static void AddSubview<TParent, TChild>(this TParent parent, TChild child, Func<TChild, TParent, IEnumerable<NSLayoutConstraint>> constraints)
            where TChild : NSView where TParent : NSView
        {
            parent.WithSubview(child, constraints);
        }
        
        public static TParent WithSubview<TParent, TChild>(this TParent parent, TChild child, Func<TChild, TParent, IEnumerable<NSLayoutConstraint>> constraints)
            where TChild : NSView where TParent : NSView
        {
            parent.AddSubview(child);
            child.TranslatesAutoresizingMaskIntoConstraints = false;
            NSLayoutConstraint.ActivateConstraints(constraints(child, parent).ToArray());
            return parent;
        }
        

        public static T WithConstraints<T>(this T view, Func<T, IEnumerable<NSLayoutConstraint>> constraints) where T : NSView
        {
            view.TranslatesAutoresizingMaskIntoConstraints = false;
            NSLayoutConstraint.ActivateConstraints(constraints(view).ToArray());
            return view;
        }

        public static T With<T>(this T view, Action<T> action) where T : NSView
        {
            action(view);
            return view;
        }
    }

    public static class NSStackViewExtensions
    {
        public static TParent WithArrangedSubviews<TParent>(this TParent parent, NSView child, params NSView[] children) where TParent : NSStackView
        {
            parent.AddArrangedSubview(child);
            foreach (var c in children)
            {
                parent.AddArrangedSubview(c);
            }
            return parent;
        }
    }
}

