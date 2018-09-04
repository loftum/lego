// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace SteeringWheel.Controllers
{
    [Register ("SteeringWheelViewController")]
    partial class SteeringWheelViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton DisconnectButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISlider Throttle { get; set; }

        [Action ("DisconnectButton_TouchUpInside:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void DisconnectButton_TouchUpInside (UIKit.UIButton sender);

        [Action ("Throttle_Cancel:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void Throttle_Cancel (UIKit.UISlider sender);

        void ReleaseDesignerOutlets ()
        {
            if (DisconnectButton != null) {
                DisconnectButton.Dispose ();
                DisconnectButton = null;
            }

            if (Throttle != null) {
                Throttle.Dispose ();
                Throttle = null;
            }
        }
    }
}