using System.Text;

namespace LCTP.Core
{
    static class Encodings
    {
        public static readonly UTF8Encoding Utf8NoBom = new UTF8Encoding(false);
    }
}