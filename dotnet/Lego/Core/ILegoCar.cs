using Maths;

namespace Lego.Core
{
    public interface ILegoCar
    {
        ILight LeftBlinker { get; }
        ILight RightBlinker { get; }
        ILight Headlights { get; }
        IServo SteerFront { get; }
        IServo SteerBack { get; }
        Vector3 GetOrientation();
        int GetMotorSpeed(int motorNumber);
        void SetMotorSpeed(int motorNumber, int speed);
        void SetMotorSpeed(int speed);
        void Reset();
        LegoCarState GetState();
    }
}