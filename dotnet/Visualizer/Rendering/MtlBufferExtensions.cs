using System.Runtime.InteropServices;
using Metal;

namespace Visualizer.Rendering
{
    public static class MtlBufferExtensions
    {
        public static void Copy<T>(this IMTLBuffer buffer, T uniforms, int bufferIndex) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var rawdata = new byte[size];

            var pinnedUniforms = GCHandle.Alloc(uniforms, GCHandleType.Pinned);
            var ptr = pinnedUniforms.AddrOfPinnedObject();
            Marshal.Copy(ptr, rawdata, 0, size);
            pinnedUniforms.Free();
            var offset = size * bufferIndex;

            Marshal.Copy(rawdata, 0, buffer.Contents + offset, size);
        }
    }
}