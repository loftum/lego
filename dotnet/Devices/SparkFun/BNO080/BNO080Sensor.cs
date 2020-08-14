using System.Collections.Generic;
using System.Linq;
using System.Resources;
using Unosquare.PiGpio.ManagedModel;

namespace Devices.SparkFun.BNO080
{
    public class BNO080Sensor
    {
        private readonly Dictionary<BNO080Channels, byte> _sequenceNumbers = new Dictionary<BNO080Channels, byte>
        {
            [BNO080Channels.Command] = 0,
            [BNO080Channels.Executable] = 0,
            [BNO080Channels.Control] = 0,
            [BNO080Channels.Reports] = 0,
            [BNO080Channels.WakeReports] = 0,
            [BNO080Channels.Gyro] = 0
        };
        
        public const byte DefaultI2CAddress = 0x4B;
        public const byte AlternativeI2CAddress = 0x4A;

        private readonly I2cDevice _device;

        public BNO080Sensor(BoardPeripheralsService bus)
        {
            _device = bus.OpenI2cDevice(DefaultI2CAddress);
            Reset();
            
        }
        
        

        public void Reset()
        {
            SendShtpPacket(BNO080Channels.Command, new byte[] { 1 });
            var bytes = ReceiveShtpPacket();
            while (bytes.Length > 0)
            {
                bytes = ReceiveShtpPacket();
            }
        }

        private void SendShtpPacket(BNO080Channels channel, byte[] value)
        {
            var length = value.Length + 4;
            var bytes = new[]
            {
                (byte) (length & 0xFF),
                (byte) (length >> 8),
                (byte) channel,
                NextSequenceNumber(channel),
            }.Concat(value);
            _device.Write(bytes.ToArray());
        }

        private byte[] ReceiveShtpPacket()
        {
            var header = _device.ReadRaw(4);
            var length = (header[0] << 8 | header[1]) & ~(1 << 15);
            if (length == 0)
            {
                return new byte[0];
            }
            return _device.ReadRaw(length - 4);
        }

        private byte NextSequenceNumber(BNO080Channels channel)
        {
            var value = _sequenceNumbers[channel]++;
            if (value == byte.MaxValue)
            {
                _sequenceNumbers[channel] = 0;
            }
            return value;
        }
    }

    public struct BNO080ProductId
    {
        public byte Major;
        public byte Minor;
        public int PartNumber;
        public int SoftwareBuildNumber;
        public short SoftwareVersionPatch;
    }

    public enum BNO080Channels
    {
        Command = 0x00,
        Executable = 0x01,
        Control = 0x02,
        Reports = 0x03,
        WakeReports = 0x04,
        Gyro = 0x05
    }
}