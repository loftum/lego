namespace Maths
{
    public struct ComposedShort
    {
        public byte Lsb { get; }
        public byte Msb { get; }
        public short ShortValue => (short) (Msb << 8 | Lsb);
        
        public ComposedShort(byte lsb, byte msb)
        {
            Lsb = lsb;
            Msb = msb;
        }

        public override string ToString()
        {
            return ToBinaryString();
        }

        public string ToBinaryString()
        {
            return $"{Msb.ToBinaryString()} {Lsb.ToBinaryString()}";
        }
    }
}