namespace Devices.Adafruit.LSM9DS1.Accelerometer
{
    public enum AccelDataRate
    {
        _POWERDOWN = (0b0000 << 4),
        _3_125HZ = (0b0001 << 4),
        _6_25HZ = (0b0010 << 4),
        _12_5HZ = (0b0011 << 4),
        _25HZ = (0b0100 << 4),
        _50HZ = (0b0101 << 4),
        _100HZ = (0b0110 << 4),
        _200HZ = (0b0111 << 4),
        _400HZ = (0b1000 << 4),
        _800HZ = (0b1001 << 4),
        _1600HZ = (0b1010 << 4)
    }
}