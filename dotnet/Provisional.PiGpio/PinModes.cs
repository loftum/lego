using System;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.RaspberryIO.Abstractions;

namespace Provisional.PiGpio
{
    internal static class PinModes
    {
        public static GpioPullMode Map(GpioPinResistorPullMode mode)
        {
            return mode switch
            {
                GpioPinResistorPullMode.Off => GpioPullMode.Off,
                GpioPinResistorPullMode.PullDown => GpioPullMode.Down,
                GpioPinResistorPullMode.PullUp => GpioPullMode.Up,
                _ => throw new InvalidOperationException($"GpioPinResistorPullMode {mode} not supported")
            };
        }
        
        public static GpioPinResistorPullMode Map(GpioPullMode mode)
        {
            return mode switch
            {
                GpioPullMode.Off => GpioPinResistorPullMode.Off,
                GpioPullMode.Down => GpioPinResistorPullMode.PullDown,
                GpioPullMode.Up => GpioPinResistorPullMode.PullUp,
                _ => throw new InvalidOperationException($"GpioPullMode {mode} not supported")
            };
        }
        
        public static GpioPinDriveMode Map(PinMode mode)
        {
            return mode switch
            {
                PinMode.Input => GpioPinDriveMode.Input,
                PinMode.Output => GpioPinDriveMode.Output,
                PinMode.Alt0 => GpioPinDriveMode.Alt0,
                PinMode.Alt1 => GpioPinDriveMode.Alt1,
                PinMode.Alt2 => GpioPinDriveMode.Alt2,
                PinMode.Alt3 => GpioPinDriveMode.Alt3,
                PinMode.Alt4 => throw new InvalidOperationException($"PinMode {mode} not supported"),
                PinMode.Alt5 => throw new InvalidOperationException($"PinMode {mode} not supported"),
                _ => throw new InvalidOperationException($"PinMode {mode} not supported")
            };
        }

        public static PinMode Map(GpioPinDriveMode mode)
        {
            return mode switch
            {
                GpioPinDriveMode.Input => PinMode.Input,
                GpioPinDriveMode.Output => PinMode.Output,
                GpioPinDriveMode.PwmOutput => throw new InvalidOperationException($"GpioPinDriveMode {mode} is not supported"),
                GpioPinDriveMode.GpioClock => throw new InvalidOperationException($"GpioPinDriveMode {mode} is not supported"),
                GpioPinDriveMode.Alt0 => PinMode.Alt0,
                GpioPinDriveMode.Alt1 => PinMode.Alt1,
                GpioPinDriveMode.Alt2 => PinMode.Alt2,
                GpioPinDriveMode.Alt3 => PinMode.Alt3,
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }
    }
}