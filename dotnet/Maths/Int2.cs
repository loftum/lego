namespace Maths
{
    public struct Int2
    {
        public int X;
        public int Y;

        public Int2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"[{X},{Y}]";
        }

        public static bool TryParse(string stringValue, out Int2 result)
        {
            result = Zero;
            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return false;
            }

            var parts = stringValue.TrimStart('[').TrimEnd(']').Split(',');
            if (parts.Length != 2)
            {
                return false;
            }

            if (int.TryParse(parts[0], out var x) &&
                int.TryParse(parts[1], out var y))
            {
                result = new Int2(x, y);
                return true;
            }
            return false;
        }
        
        public static Int2 Zero => new Int2(0, 0);
    }
}