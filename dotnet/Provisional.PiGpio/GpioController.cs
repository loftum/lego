using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unosquare.PiGpio;
using Unosquare.RaspberryIO.Abstractions;

namespace Provisional.PiGpio
{
    /// <summary>
    /// Represents the Raspberry Pi GPIO controller
    /// as an IReadOnlyCollection of GpioPins
    /// Low level operations are accomplished by using the PiGPIO library.
    /// </summary>
    public class GpioController : IGpioController
    {
        private static readonly object SyncRoot = new object();
        private readonly List<GpioPinProxy> _pins = Board.Pins.Select(p => new GpioPinProxy(p.Value)).ToList();


        /// <inheritdoc />
        public IEnumerator<IGpioPin> GetEnumerator() => _pins.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public int Count => _pins.Count;

        /// <inheritdoc />
        public IGpioPin this[int bcmPinNumber] => _pins.FirstOrDefault(p => p.BcmPinNumber == bcmPinNumber);

        /// <inheritdoc />
        public IGpioPin this[BcmPin bcmPin] => _pins.FirstOrDefault(p => p.BcmPin == bcmPin);

        /// <inheritdoc />
        public IGpioPin this[P1 pinNumber] => throw new NotImplementedException();

        /// <inheritdoc />
        public IGpioPin this[P5 pinNumber] => throw new NotImplementedException();
    }
}
