namespace LegoCar.LSM9DS1
{
    public enum MagGain
    {
        _4GAUSS = (0b00 << 5),  // +/- 4 gauss
        _8GAUSS = (0b01 << 5),  // +/- 8 gauss
        _12GAUSS = (0b10 << 5),  // +/- 12 gauss
        _16GAUSS = (0b11 << 5)   // +/- 16 gauss
    }
}