using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.PiGpio.ManagedModel;
using Unosquare.PiGpio.NativeEnums;

namespace Devices.PixArt
{
    public abstract class PimoroniFlowSensor : IDisposable
    {
        protected const byte WAIT = 0xff;
        protected const byte REG_POWER_UP_RESET = 0x3a;
        protected const byte REG_DATA_READY = 0x02;
        protected const byte REG_ID = 0x00;
        protected const byte REG_ORIENTATION = 0x5b;
        protected const byte REG_MOTION_BURST = 0x16;
        protected const byte SecretRegister = 0x7f;

        protected readonly SpiChannel SpiChannel;
        protected readonly GpioPin CsPin;

        protected PimoroniFlowSensor(SpiChannel spiChannel, GpioPin csPin)
        {
            SpiChannel = spiChannel;
            csPin.Direction = PinDirection.Output;
            CsPin = csPin;
        }

        protected abstract void SecretSauce();

        public async Task InitAsync(CancellationToken cancellationToken)
        {
            CsPin.Value = false;
            await Task.Delay(50, cancellationToken);
            CsPin.Value = true;
            Write(REG_POWER_UP_RESET, 0x5a);
            await Task.Delay(20, cancellationToken);
            
            for (byte ii = 0; ii < 5; ii++)
            {
                Read((byte) (REG_DATA_READY + ii));
            }

            var (productId, revision) = GetId();
            if (productId != 0x49 || revision != 0x00)
            {
                throw new Exception($"Invalid productId/revision: {productId}/{revision}");
            }
            SecretSauce();
        }

        private (byte, byte) GetId()
        {
            var read = Read(REG_ID, 2);
            return (read[0], read[1]);
        }

        public void SetRotation(int degrees)
        {
            switch (degrees)
            {
                case 0:
                    SetOrientation(true, true, true);
                    break;
                case 90:
                    SetOrientation(false, true, false);
                    break;
                case 180:
                    SetOrientation(false, false, true);
                    break;
                case 270:
                    SetOrientation(true, false, false);
                    break;
                default:
                    throw new Exception("Degrees must be 0, 90, 180 or 270");
            }
        }

        private void SetOrientation(bool invertX, bool invertY, bool swapXy)
        {
            byte value = 0;
            if (swapXy)
            {
                value |= 0b10000000;
            }

            if (invertY)
            {
                value |= 0b01000000;
            }

            if (invertX)
            {
                value |= 0b00100000;
            }

            Write(REG_ORIENTATION, value);
        }

        // 
        private static readonly byte[] MotionBurstBytes = { REG_MOTION_BURST, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }; 
        
        public async Task<(short, short)> GetMotionAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var data = Transfer(MotionBurstBytes);
                var v = data[0];
                var dr = data[1];
                var obs = data[2];
                var x = (short) (data[3] | data[4] << 8);
                var y = (short) (data[5] | data[6] << 8);
                var quality = data[7];
                var raw_sum = data[8];
                var raw_max = data[9];
                var raw_min = data[10];
                var shutter_upper = data[11];
                var shutter_lower = data[12];
                
                //Console.WriteLine($"v: {v}, dr: {dr}, obs: {obs}, x: {x}, y:{y}, quality: {quality}, raw_sum: {raw_sum}, raw_max: {raw_max}, raw_min: {raw_min}, shutter_upper: {shutter_upper}, shutter_lower: {shutter_lower}");

                if ((dr & 0b1000_0000) != 0 && !(quality < 0x19 && shutter_upper == 0x1f))
                {
                    return (x, y);
                }

                await Task.Delay(10, cancellationToken);
            }

            return (0, 0);
        }

        protected IList<byte> Read(byte register, int length)
        {
            var bytes = new List<byte>();
            for (var ii = 0; ii < length; ii++)
            {
                var read = Transfer(new[] { (byte)(register + ii), (byte)0x00 });
                bytes.Add(read[1]);
            }

            return bytes;
        }
        
        protected byte Read(byte register)
        {
            var read = Transfer(new[] { register, (byte)0x00 });
            return read[1];
        }

        protected void BulkWrite(byte[] data)
        {
            for (var ii = 0; ii < data.Length; ii += 2)
            {
                var register = data[ii];
                var value = data[ii + 1];
                if (register == WAIT)
                {
                    Thread.Sleep(value);
                }
                else
                {
                    Write(register, value);
                }
            }
        }

        protected void Write(byte register, byte value)
        {
            CsPin.Value = false;
            SpiChannel.Write(new[] { (byte) (register | 0x80), value });
            CsPin.Value = true;
        }

        private byte[] Transfer(byte[] data)
        {
            CsPin.Value = false;
            var read = SpiChannel.Transfer(data);
            CsPin.Value = true;
            return read;
        }

        public void Dispose()
        {
            BulkWrite(new byte[]
            {
                SecretRegister, 0x14,  
                0x6f, 0x00 // Turn off LED (I think)
            });
            
            Write(0x3b, 0x00); // Shutdown (I think)
        }
    }
}