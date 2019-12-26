using Devices.ThePiHut.ServoPWMPiZero;

namespace Lego.Core
{
    public interface ILegoCar
    {
        ILight LeftBlinker { get; }
        ILight RightBlinker { get; }
        ILight Headlights { get; }
        IServo SteerFront { get; }
        IServo SteerBack { get; }
        int GetMotorSpeed(int motorNumber);
        void SetMotorSpeed(int motorNumber, int speed);
        void SetMotorSpeed(int speed);
        void Reset();
    }
}