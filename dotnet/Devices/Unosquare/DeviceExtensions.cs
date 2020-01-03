using System.Collections.Generic;
using Unosquare.RaspberryIO.Abstractions;

namespace Devices.Unosquare
{
    public static class DeviceExtensions
    {
        public static byte[] ReadBlock(this II2CDevice device, int startAddress, int numberOfBytes)
        {
            var bytes = new byte[numberOfBytes];
            for (var ii = 0; ii < numberOfBytes; ii++)
            {
                bytes[ii] = device.ReadAddressByte(startAddress + ii);
            }
            return bytes;
        }
        
        public static void WriteBlock(this II2CDevice device, int startAddress, IEnumerable<byte> bytes)
        {
            var ii = 0;
            foreach (var b in bytes)
            {
                device.WriteAddressByte(startAddress + ii, b);
                ii++;
            }
        }
    }
}