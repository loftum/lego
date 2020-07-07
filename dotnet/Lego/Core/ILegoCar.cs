using Maths;

namespace Lego.Core
{
    public interface ILegoCar
    {
        void SetThrottle(int speed);
        void SetSteerAngle(int angle);
        void Reset();
        LegoCarState GetState();
        void StopEngine();
        void StartEngine();
    }

    public interface IRedCar : ILegoCar
    {
        Double3 GetEulerAngles();
        Quatd GetQuaternion();
        ILight LeftBlinker { get; }
        ILight RightBlinker { get; }
        ILight Headlights { get; }
    }
}