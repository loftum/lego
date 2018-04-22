namespace LegoCar.LSM9DS1
{
    public enum GyroScale
    {
        _245DPS = (0b00 << 3),  // +/- 245 degrees per second rotation
        _500DPS = (0b01 << 3),  // +/- 500 degrees per second rotation
        _2000DPS = (0b11 << 3)   // +/- 2000 degrees per second rotation
    }
}