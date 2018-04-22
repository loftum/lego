namespace LegoCar.LSM9DS1
{
    public enum AccelRange
    {
        _2G = (0b00 << 3),
        _16G = (0b01 << 3),
        _4G = (0b10 << 3),
        _8G = (0b11 << 3),
    }
}