namespace LegoCar.LSM9DS1
{
    public enum MagDataRate
    {
        _3_125HZ = (0b000 << 2),
        _6_25HZ = (0b001 << 2),
        _12_5HZ = (0b010 << 2),
        _25HZ = (0b011 << 2),
        _50HZ = (0b100 << 2),
        _100HZ = (0b101 << 2)
    }
}