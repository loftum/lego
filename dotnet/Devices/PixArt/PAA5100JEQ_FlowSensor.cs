using System;
using Unosquare.PiGpio.ManagedModel;

namespace Devices.PixArt
{
    /// <summary>
    /// 3-5V -> 5V or 3V
    /// GND -> GND
    /// 
    /// CS -> BCM7
    /// SCK -> BCM11
    /// MOSI -> BCM10
    /// MISO -> BCM9
    /// INT -> BCM19
    /// 
    /// </summary>
    public class PAA5100JEQ_FlowSensor : PimoroniFlowSensor
    {
        public PAA5100JEQ_FlowSensor(SpiChannel spiChannel, GpioPin csPin) : base(spiChannel, csPin)
        {
        }

        protected override void SecretSauce()
        {
            BulkWrite(new byte[]{
                0x7f, 0x00,
                0x55, 0x01,
                0x50, 0x07,

                0x7f, 0x0e,
                0x43, 0x10
            });

            if ((Read(0x67) & 0b1000_0000) != 0x00)
            {
                Write(0x48, 0x04);
            }
            else
            {
                Write(0x48, 0x02);
            }
            
            
            BulkWrite(new byte[]{
                0x7f, 0x00,
                0x51, 0x7b,

                0x50, 0x00,
                0x55, 0x00,
                0x7f, 0x0E
            });

            if (Read(0x73) == 0x00)
            {
                var c1 = Read(0x70);
                var c2 = Read(0x71);
                if (c1 <= 28)
                {
                    c1 += 14;
                }

                if (c1 > 28)
                {
                    c1 += 11;
                }

                c1 = Math.Max((byte) 0x00, Math.Min((byte)0x3F, c1));
                c2 = (byte) (c2 * 45 / 100);
                BulkWrite(new byte[]{
                    0x7f, 0x00,
                    0x61, 0xad,
                    0x51, 0x70,
                    0x7f, 0x0e
                });
                Write(0x70, c1);
                Write(0x71, c2);
            }

            BulkWrite(new byte[]
            {
                0x7f, 0x00,
                0x61, 0xad,

                0x7f, 0x03,
                0x40, 0x00,

                0x7f, 0x05,
                0x41, 0xb3,
                0x43, 0xf1,
                0x45, 0x14,

                0x5f, 0x34,
                0x7b, 0x08,
                0x5e, 0x34,
                0x5b, 0x11,
                0x6d, 0x11,
                0x45, 0x17,
                0x70, 0xe5,
                0x71, 0xe5,

                0x7f, 0x06,
                0x44, 0x1b,
                0x40, 0xbf,
                0x4e, 0x3f,

                0x7f, 0x08,
                0x66, 0x44,
                0x65, 0x20,
                0x6a, 0x3a,
                0x61, 0x05,
                0x62, 0x05,

                0x7f, 0x09,
                0x4f, 0xaf,
                0x5f, 0x40,
                0x48, 0x80,
                0x49, 0x80,
                0x57, 0x77,
                0x60, 0x78,
                0x61, 0x78,
                0x62, 0x08,
                0x63, 0x50,

                0x7f, 0x0a,
                0x45, 0x60,

                0x7f, 0x00,
                0x4d, 0x11,
                0x55, 0x80,
                0x74, 0x21,
                0x75, 0x1f,
                0x4a, 0x78,
                0x4b, 0x78,
                0x44, 0x08,

                0x45, 0x50,
                0x64, 0xff,
                0x65, 0x1f,

                0x7f, 0x14,
                0x65, 0x67,
                0x66, 0x08,
                0x63, 0x70,
                0x6f, 0x1c,

                0x7f, 0x15,
                0x48, 0x48,

                0x7f, 0x07,
                0x41, 0x0d,
                0x43, 0x14,
                0x4b, 0x0e,
                0x45, 0x0f,
                0x44, 0x42,
                0x4c, 0x80,

                0x7f, 0x10,
                0x5b, 0x02,

                0x7f, 0x07,
                0x40, 0x41,

                WAIT, 0x0a, // Wait 10ms

                0x7f, 0x00,
                0x32, 0x00,

                0x7f, 0x07,
                0x40, 0x40,

                0x7f, 0x06,
                0x68, 0xf0,
                0x69, 0x00,

                0x7f, 0x0d,
                0x48, 0xc0,
                0x6f, 0xd5,

                0x7f, 0x00,
                0x5b, 0xa0,
                0x4e, 0xa8,
                0x5a, 0x90,
                0x40, 0x80,
                0x73, 0x1f,

                WAIT, 0x0a, // Wait 10ms

                0x73, 0x00
            });
        }
    }
}