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
        Double3 GetEulerAngles();
        Quatd GetQuaternion();
        void SetThrottle(int speed);
        void SetSteerAngle(int angle);
        void Reset();
        LegoCarState GetState();
        void StopEngine();
        void StartEngine();
    }
}