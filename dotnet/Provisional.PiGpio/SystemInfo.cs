using System;
using Unosquare.PiGpio.NativeEnums;
using Unosquare.PiGpio.NativeMethods;
using Unosquare.RaspberryIO.Abstractions;

namespace Provisional.PiGpio
{
    /// <summary>
    /// Represents the system info.
    /// </summary>
    /// <seealso cref="ISystemInfo" />
    public class SystemInfo : ISystemInfo
    {
        private static readonly Version _libVersion;
        private static readonly BoardRevision _boardRevision = BoardRevision.Rev2;

        static SystemInfo()
        {
            HardwareRevision = Utilities.GpioHardwareRevision();
            _libVersion = new Version(Convert.ToInt32((uint) Utilities.GpioVersion()), 0);
            _boardRevision = Constants.GetBoardRevision(HardwareRevision);
            BoardType = Constants.GetBoardType(HardwareRevision);
        }

        /// <summary>
        /// Gets the hardware revision number.
        /// </summary>
        public static long HardwareRevision { get; }

        /// <summary>
        /// Gets the type of the board. See the <see cref="Unosquare.PiGpio.NativeEnums.BoardType"/> enumeration.
        /// </summary>
        public static BoardType BoardType { get; }

        /// <inheritdoc />
        public BoardRevision BoardRevision => _boardRevision;

        /// <inheritdoc />
        public Version LibraryVersion => _libVersion;
    }
}
