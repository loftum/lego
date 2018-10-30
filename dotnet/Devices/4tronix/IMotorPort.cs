namespace Devices._4tronix
{
    public interface IMotorPort
    {
        int MinSpeed { get; set; }
        int MaxSpeed { get; set; }
        int Speed { get; set; }
    }
}