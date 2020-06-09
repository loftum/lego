using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Maths;

namespace Lego.Server
{
    public class SpeedSensor : IDisposable
    {
        private const string FileName = "/dev/input/mice";
        
        private readonly Stream _stream;

        public SpeedSensor()
        {
            _stream = File.OpenRead(FileName);
        }

        public async Task<Int2> GetSpeedAsync(CancellationToken cancellationToken = default)
        {
            var buffer = new byte[3];
            var read = await _stream.ReadAsync(buffer, 0, 3, cancellationToken);
            if (read < 0)
            {
                return Int2.Zero;
            }

            var x = (sbyte) buffer[1];
            var y = (sbyte) buffer[2];
            // coordinates from mouse are swapped.
            return new Int2(y, x);
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}