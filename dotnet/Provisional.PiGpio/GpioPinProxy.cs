using System;
using Swan.Diagnostics;
using Unosquare.PiGpio.ManagedModel;
using Unosquare.PiGpio.NativeMethods;
using Unosquare.RaspberryIO.Abstractions;
using EdgeDetection = Unosquare.RaspberryIO.Abstractions.EdgeDetection;

namespace Provisional.PiGpio
{
    internal class GpioPinProxy : IGpioPin
    {
        private readonly GpioPin _pin;

        public GpioPinProxy(GpioPin pin)
        {
            _pin = pin;
        }

        public bool Read() => _pin.Read() == 1;

        public void Write(bool value) => _pin.Write(value ? 1 : 0);

        public void Write(GpioPinValue value) => Write(value == GpioPinValue.High);

        public bool WaitForValue(GpioPinValue status, int timeOutMillisecond)
        {
            if (_pin.Mode != Unosquare.PiGpio.NativeEnums.PinMode.Input)
            {
                throw new InvalidOperationException($"Cannot read from pin {BcmPinNumber}. Mode is {PinMode}");
            }
            var highResolutionTimer = new HighResolutionTimer();
            highResolutionTimer.Start();
            var expected = status == GpioPinValue.High ? 1 : 0;
            while (_pin.Read() != expected)
            {
                if (highResolutionTimer.ElapsedMilliseconds > timeOutMillisecond)
                {
                    return false;
                }
            }
            return true;
        }

        public void RegisterInterruptCallback(EdgeDetection edgeDetection, Action callback)
        {
            throw new NotImplementedException();
        }

        public void RegisterInterruptCallback(EdgeDetection edgeDetection, Action<int, int, uint> callback)
        {
            throw new NotImplementedException();
        }

        public BcmPin BcmPin => (BcmPin) _pin.PinNumber;
        public int BcmPinNumber => _pin.PinNumber;
        public int PhysicalPinNumber => (int) _pin.PinGpio; // maybe ?
        public GpioHeader Header => throw new NotImplementedException();

        public GpioPinDriveMode PinMode
        {
            get => PinModes.Map(_pin.Mode);
            set => IO.GpioSetMode(_pin.PinGpio, PinModes.Map(value));
        }

        public GpioPinResistorPullMode InputPullMode
        {
            get => PinModes.Map(_pin.PullMode);
            set => _pin.PullMode = PinModes.Map(value);
        }

        public bool Value
        {
            get => _pin.Value;
            set => _pin.Value = value;
        }
    }
}